from pydantic import BaseModel


class Landmark(BaseModel):
    x: float
    y: float
    z: float
    visibility: float


class FrameResult(BaseModel):
    frame_index: int
    landmarks: list[Landmark]


class VideoAnalysisResult(BaseModel):
    frame_count: int
    fps: float
    frames: list[FrameResult]


class FrameAngles(BaseModel):
    frame_index: int
    angles: dict[str, float]


class ExerciseAnalysisResult(BaseModel):
    frame_count: int
    fps: float
    exercise_type: str
    score: int
    rep_count: int
    frame_angles: list[FrameAngles]
    feedback: list[str]
