import os
import tempfile

from fastapi import APIRouter, File, Form, HTTPException, UploadFile

from app.analysis import ANALYSERS
from app.models.pose_models import ExerciseAnalysisResult
from app.services.pose_service import PoseService

router = APIRouter()
_pose_service = PoseService()


@router.get("/health")
async def health() -> dict[str, str]:
    return {"status": "ok"}


@router.post("/analyze-exercise", response_model=ExerciseAnalysisResult)
async def analyze_exercise(
    file: UploadFile = File(...),
    exercise_type: str = Form(...),
) -> ExerciseAnalysisResult:
    analyser = ANALYSERS.get(exercise_type)
    if analyser is None:
        raise HTTPException(
            status_code=400,
            detail=f"Unsupported exercise '{exercise_type}'. Supported: {sorted(ANALYSERS)}",
        )

    tmp_path: str | None = None
    try:
        with tempfile.NamedTemporaryFile(delete=False, suffix=".mp4") as tmp:
            tmp.write(await file.read())
            tmp_path = tmp.name

        video = _pose_service.analyze(tmp_path)
        return analyser.analyse(video.frames, video.frame_count, video.fps)
    except ValueError as exc:
        raise HTTPException(status_code=400, detail=str(exc)) from exc
    finally:
        if tmp_path and os.path.exists(tmp_path):
            os.unlink(tmp_path)
