import cv2
import mediapipe as mp

from app.models.pose_models import FrameResult, Landmark, VideoAnalysisResult


class PoseService:
    def __init__(self) -> None:
        self._pose = mp.solutions.pose.Pose(
            static_image_mode=False,
            model_complexity=1,
            enable_segmentation=False,
            min_detection_confidence=0.5,
            min_tracking_confidence=0.5,
        )

    def analyze(self, video_path: str) -> VideoAnalysisResult:
        cap = cv2.VideoCapture(video_path)
        if not cap.isOpened():
            raise ValueError(f"Cannot open video file: {video_path}")

        fps: float = cap.get(cv2.CAP_PROP_FPS) or 30.0
        frames: list[FrameResult] = []
        frame_index = 0

        try:
            while True:
                ret, frame = cap.read()
                if not ret:
                    break

                rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
                result = self._pose.process(rgb)

                if result.pose_landmarks:
                    landmarks = [
                        Landmark(x=lm.x, y=lm.y, z=lm.z, visibility=lm.visibility)
                        for lm in result.pose_landmarks.landmark
                    ]
                    frames.append(FrameResult(frame_index=frame_index, landmarks=landmarks))

                frame_index += 1
        finally:
            cap.release()

        return VideoAnalysisResult(frame_count=frame_index, fps=fps, frames=frames)
