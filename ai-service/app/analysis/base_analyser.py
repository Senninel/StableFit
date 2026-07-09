from abc import ABC, abstractmethod

from app.models.pose_models import ExerciseAnalysisResult, FrameResult


class BaseExerciseAnalyser(ABC):
    @abstractmethod
    def analyse(
        self,
        frames: list[FrameResult],
        frame_count: int,
        fps: float,
    ) -> ExerciseAnalysisResult:
        ...
