from app.analysis.angle_utils import calculate_angle
from app.analysis.base_analyser import BaseExerciseAnalyser
from app.models.pose_models import ExerciseAnalysisResult, FrameAngles, FrameResult

_L_SHOULDER, _R_SHOULDER = 11, 12
_L_HIP, _R_HIP = 23, 24
_L_ANKLE, _R_ANKLE = 27, 28

_GOOD_ALIGNMENT = 160.0


class PlankAnalyser(BaseExerciseAnalyser):
    def analyse(
        self,
        frames: list[FrameResult],
        frame_count: int,
        fps: float,
    ) -> ExerciseAnalysisResult:
        frame_angles = self._compute_angles(frames)
        score = self._score(frame_angles)
        feedback = self._feedback(frame_angles, frame_count, fps)

        return ExerciseAnalysisResult(
            frame_count=frame_count,
            fps=fps,
            exercise_type="plank",
            score=score,
            rep_count=0,
            frame_angles=frame_angles,
            feedback=feedback,
        )

    def _compute_angles(self, frames: list[FrameResult]) -> list[FrameAngles]:
        result: list[FrameAngles] = []
        for frame in frames:
            lms = frame.landmarks
            if len(lms) < 29:
                continue

            def pt(idx: int) -> tuple[float, float]:
                return (lms[idx].x, lms[idx].y)

            left_align = calculate_angle(pt(_L_SHOULDER), pt(_L_HIP), pt(_L_ANKLE))
            right_align = calculate_angle(pt(_R_SHOULDER), pt(_R_HIP), pt(_R_ANKLE))

            l_mid_y = (lms[_L_SHOULDER].y + lms[_L_ANKLE].y) / 2
            r_mid_y = (lms[_R_SHOULDER].y + lms[_R_ANKLE].y) / 2
            hip_offset = round(((lms[_L_HIP].y - l_mid_y) + (lms[_R_HIP].y - r_mid_y)) / 2, 3)

            result.append(FrameAngles(
                frame_index=frame.frame_index,
                angles={
                    "alignment_angle": round((left_align + right_align) / 2, 1),
                    "hip_offset": hip_offset,
                },
            ))
        return result

    def _score(self, frame_angles: list[FrameAngles]) -> int:
        if not frame_angles:
            return 0
        well_aligned = sum(
            1 for fa in frame_angles
            if fa.angles["alignment_angle"] >= _GOOD_ALIGNMENT
        )
        return round(100 * well_aligned / len(frame_angles))

    def _feedback(
        self,
        frame_angles: list[FrameAngles],
        frame_count: int,
        fps: float,
    ) -> list[str]:
        msgs: list[str] = []

        if not frame_angles:
            msgs.append("No pose detected — ensure your full body is visible.")
            return msgs

        duration = frame_count / fps if fps > 0 else 0
        msgs.append(f"Hold duration: {duration:.1f} seconds.")

        offsets = [fa.angles["hip_offset"] for fa in frame_angles]
        pct_sagging = 100 * sum(1 for o in offsets if o > 0.05) / len(offsets)
        pct_raised = 100 * sum(1 for o in offsets if o < -0.05) / len(offsets)

        if pct_sagging > 20:
            msgs.append(
                f"Hips are sagging {pct_sagging:.0f}% of the time — "
                "engage your core and glutes to hold the position."
            )
        elif pct_raised > 20:
            msgs.append(
                f"Hips are too high {pct_raised:.0f}% of the time — "
                "lower them to form a straight line from head to heels."
            )
        else:
            msgs.append("Good alignment — body is staying straight throughout.")

        return msgs
