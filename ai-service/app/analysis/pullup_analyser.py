from app.analysis.angle_utils import calculate_angle
from app.analysis.base_analyser import BaseExerciseAnalyser
from app.models.pose_models import ExerciseAnalysisResult, FrameAngles, FrameResult

_L_SHOULDER, _R_SHOULDER = 11, 12
_L_ELBOW, _R_ELBOW = 13, 14
_L_WRIST, _R_WRIST = 15, 16

_TOP_THRESHOLD = 80.0      # chin over bar — elbows fully bent
_BOTTOM_THRESHOLD = 150.0  # dead hang — arms fully extended


class PullupAnalyser(BaseExerciseAnalyser):
    def analyse(
        self,
        frames: list[FrameResult],
        frame_count: int,
        fps: float,
    ) -> ExerciseAnalysisResult:
        frame_angles = self._compute_angles(frames)
        rep_count, rep_data = self._count_reps(frame_angles)
        score = self._score(rep_data)
        feedback = self._feedback(rep_count, rep_data)

        return ExerciseAnalysisResult(
            frame_count=frame_count,
            fps=fps,
            exercise_type="pullup",
            score=score,
            rep_count=rep_count,
            frame_angles=frame_angles,
            feedback=feedback,
        )

    def _compute_angles(self, frames: list[FrameResult]) -> list[FrameAngles]:
        result: list[FrameAngles] = []
        for frame in frames:
            lms = frame.landmarks
            if len(lms) < 17:
                continue

            def pt(idx: int) -> tuple[float, float]:
                return (lms[idx].x, lms[idx].y)

            left_elbow = calculate_angle(pt(_L_SHOULDER), pt(_L_ELBOW), pt(_L_WRIST))
            right_elbow = calculate_angle(pt(_R_SHOULDER), pt(_R_ELBOW), pt(_R_WRIST))

            result.append(FrameAngles(
                frame_index=frame.frame_index,
                angles={"elbow_angle": round((left_elbow + right_elbow) / 2, 1)},
            ))
        return result

    def _count_reps(
        self, frame_angles: list[FrameAngles]
    ) -> tuple[int, list[tuple[float, float]]]:
        reps = 0
        rep_data: list[tuple[float, float]] = []  # (top_angle, bottom_angle)
        at_top = False
        current_min = 180.0
        current_max = 0.0

        for fa in frame_angles:
            angle = fa.angles["elbow_angle"]
            if not at_top and angle < _TOP_THRESHOLD:
                at_top = True
                current_min = angle
                current_max = angle
            elif at_top:
                current_min = min(current_min, angle)
                current_max = max(current_max, angle)
                if angle > _BOTTOM_THRESHOLD:
                    reps += 1
                    rep_data.append((current_min, current_max))
                    at_top = False
                    current_min = 180.0
                    current_max = 0.0

        return reps, rep_data

    def _score(self, rep_data: list[tuple[float, float]]) -> int:
        if not rep_data:
            return 0
        good_top = sum(1 for top, _ in rep_data if top <= 80)
        good_bottom = sum(1 for _, bottom in rep_data if bottom >= 150)
        top_score = 50 * good_top / len(rep_data)
        bottom_score = 50 * good_bottom / len(rep_data)
        return round(top_score + bottom_score)

    def _feedback(
        self,
        rep_count: int,
        rep_data: list[tuple[float, float]],
    ) -> list[str]:
        msgs: list[str] = []

        if rep_count == 0:
            msgs.append(
                "No complete repetitions detected — ensure your full body is visible "
                "and you fully extend your arms at the bottom."
            )
            return msgs

        avg_top = sum(t for t, _ in rep_data) / len(rep_data)
        avg_bottom = sum(b for _, b in rep_data) / len(rep_data)

        if avg_top <= 70:
            msgs.append("Good height — pulling effectively to chin-over-bar position.")
        else:
            msgs.append(
                f"Pull higher — average top position is {avg_top:.0f}° (aim for under 80°)."
            )

        if avg_bottom >= 150:
            msgs.append("Good dead hang — fully extending arms at the bottom.")
        else:
            msgs.append(
                f"Extend fully at the bottom — "
                f"avg extension is {avg_bottom:.0f}° (aim for 150°+)."
            )

        return msgs
