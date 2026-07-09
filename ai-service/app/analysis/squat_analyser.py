from app.analysis.angle_utils import calculate_angle, torso_lean_angle
from app.analysis.base_analyser import BaseExerciseAnalyser
from app.models.pose_models import ExerciseAnalysisResult, FrameAngles, FrameResult

_L_SHOULDER, _R_SHOULDER = 11, 12
_L_HIP, _R_HIP = 23, 24
_L_KNEE, _R_KNEE = 25, 26
_L_ANKLE, _R_ANKLE = 27, 28

_STANDING_THRESHOLD = 160.0
_BOTTOM_THRESHOLD = 120.0


class SquatAnalyser(BaseExerciseAnalyser):
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
            exercise_type="squat",
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

            l_torso = torso_lean_angle(pt(_L_SHOULDER), pt(_L_HIP))
            r_torso = torso_lean_angle(pt(_R_SHOULDER), pt(_R_HIP))

            result.append(FrameAngles(
                frame_index=frame.frame_index,
                angles={
                    "knee_angle": round((left_knee + right_knee) / 2, 1),
                    "torso_angle": round((l_torso + r_torso) / 2, 1),
                },
            ))
        return result

    def _count_reps(
        self, frame_angles: list[FrameAngles]
    ) -> tuple[int, list[float]]:
        reps = 0
        rep_mins: list[float] = []
        in_squat = False
        current_min = 180.0

        for fa in frame_angles:
            angle = fa.angles["knee_angle"]
            if not in_squat and angle < _BOTTOM_THRESHOLD:
                in_squat = True
                current_min = angle
            elif in_squat:
                current_min = min(current_min, angle)
                if angle > _STANDING_THRESHOLD:
                    reps += 1
                    rep_mins.append(current_min)
                    in_squat = False
                    current_min = 180.0

        return reps, rep_mins

    def _score(self, rep_min_angles: list[float]) -> int:
        if not rep_min_angles:
            return 0
        avg_min = sum(rep_min_angles) / len(rep_min_angles)
        if avg_min <= 90:
            return 100
        if avg_min <= 100:
            return 90
        if avg_min <= 110:
            return 75
        if avg_min <= 120:
            return 60
        if avg_min <= 135:
            return 40
        return 20

    def _feedback(
        self,
        rep_count: int,
        rep_min_angles: list[float],
        frame_angles: list[FrameAngles],
    ) -> list[str]:
        msgs: list[str] = []

        if rep_count == 0:
            msgs.append(
                "No complete repetitions detected — ensure your full body is visible "
                "and you return to a standing position between reps."
            )
            return msgs

        avg_min_knee = sum(rep_min_angles) / len(rep_min_angles)
        if avg_min_knee <= 90:
            msgs.append("Great depth — reaching parallel or below on each rep.")
        elif avg_min_knee <= 110:
            msgs.append(
                f"Decent depth — average knee angle at the bottom was {avg_min_knee:.0f}°. "
                "Try to reach 90° or below."
            )
        else:
            msgs.append(
                f"Go deeper — average knee angle at the bottom was {avg_min_knee:.0f}°. "
                "Aim for 90° or less for a full squat."
            )

        avg_torso = sum(fa.angles["torso_angle"] for fa in frame_angles) / len(frame_angles)
        if avg_torso > 45:
            msgs.append(
                f"Torso is leaning too far forward (avg {avg_torso:.0f}° from vertical). "
                "Keep your chest up and engage your core."
            )
        else:
            msgs.append("Good torso position — back is staying relatively upright.")

        return msgs
