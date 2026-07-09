from app.analysis.angle_utils import calculate_angle, torso_lean_angle
from app.analysis.base_analyser import BaseExerciseAnalyser
from app.models.pose_models import ExerciseAnalysisResult, FrameAngles, FrameResult

_L_SHOULDER, _R_SHOULDER = 11, 12
_L_HIP, _R_HIP = 23, 24
_L_KNEE, _R_KNEE = 25, 26
_L_ANKLE, _R_ANKLE = 27, 28

_BOTTOM_THRESHOLD = 110.0  # front knee at bottom of lunge
_TOP_THRESHOLD = 160.0     # standing


class LungeAnalyser(BaseExerciseAnalyser):
    def analyse(
        self,
        frames: list[FrameResult],
        frame_count: int,
        fps: float,
    ) -> ExerciseAnalysisResult:
        frame_angles = self._compute_angles(frames)
        rep_count, rep_min_angles = self._count_reps(frame_angles)
        score = self._score(rep_min_angles)
        feedback = self._feedback(rep_count, rep_min_angles, frame_angles)

        return ExerciseAnalysisResult(
            frame_count=frame_count,
            fps=fps,
            exercise_type="lunge",
            score=score,
            rep_count=rep_count,
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

            left_knee = calculate_angle(pt(_L_HIP), pt(_L_KNEE), pt(_L_ANKLE))
            right_knee = calculate_angle(pt(_R_HIP), pt(_R_KNEE), pt(_R_ANKLE))
            front_knee = min(left_knee, right_knee)  # the more bent knee = front leg

            l_torso = torso_lean_angle(pt(_L_SHOULDER), pt(_L_HIP))
            r_torso = torso_lean_angle(pt(_R_SHOULDER), pt(_R_HIP))

            result.append(FrameAngles(
                frame_index=frame.frame_index,
                angles={
                    "front_knee_angle": round(front_knee, 1),
                    "torso_angle": round((l_torso + r_torso) / 2, 1),
                },
            ))
        return result

    def _count_reps(
        self, frame_angles: list[FrameAngles]
    ) -> tuple[int, list[float]]:
        reps = 0
        rep_mins: list[float] = []
        in_lunge = False
        current_min = 180.0

        for fa in frame_angles:
            angle = fa.angles["front_knee_angle"]
            if not in_lunge and angle < _BOTTOM_THRESHOLD:
                in_lunge = True
                current_min = angle
            elif in_lunge:
                current_min = min(current_min, angle)
                if angle > _TOP_THRESHOLD:
                    reps += 1
                    rep_mins.append(current_min)
                    in_lunge = False
                    current_min = 180.0

        return reps, rep_mins

    def _score(self, rep_min_angles: list[float]) -> int:
        if not rep_min_angles:
            return 0
        avg_min = sum(rep_min_angles) / len(rep_min_angles)
        if avg_min <= 90:
            return 100
        if avg_min <= 100:
            return 85
        if avg_min <= 110:
            return 70
        if avg_min <= 120:
            return 55
        return 30

    def _feedback(
        self,
        rep_count: int,
        rep_min_angles: list[float],
        frame_angles: list[FrameAngles],
    ) -> list[str]:
        msgs: list[str] = []

        if rep_count == 0:
            msgs.append(
                "No complete repetitions detected — step forward fully "
                "and return to standing between reps."
            )
            return msgs

        avg_min = sum(rep_min_angles) / len(rep_min_angles)
        if avg_min <= 90:
            msgs.append("Great depth — front knee reaching 90° or below.")
        elif avg_min <= 110:
            msgs.append(
                f"Good depth — try to get the front knee to 90° "
                f"(current avg: {avg_min:.0f}°)."
            )
        else:
            msgs.append(
                f"Go deeper — front knee only reaching {avg_min:.0f}° on average. Aim for 90°."
            )

        avg_torso = sum(fa.angles["torso_angle"] for fa in frame_angles) / len(frame_angles)
        if avg_torso > 40:
            msgs.append("Keep your torso upright — avoid leaning forward during the lunge.")
        else:
            msgs.append("Good upright torso position.")

        return msgs
