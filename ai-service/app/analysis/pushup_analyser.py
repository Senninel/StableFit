from app.analysis.angle_utils import calculate_angle
from app.analysis.base_analyser import BaseExerciseAnalyser
from app.models.pose_models import ExerciseAnalysisResult, FrameAngles, FrameResult

_L_SHOULDER, _R_SHOULDER = 11, 12
_L_ELBOW, _R_ELBOW = 13, 14
_L_WRIST, _R_WRIST = 15, 16
_L_HIP, _R_HIP = 23, 24
_L_ANKLE, _R_ANKLE = 27, 28

_TOP_THRESHOLD = 150.0    # elbow angle — arms extended (top of push-up)
_BOTTOM_THRESHOLD = 90.0  # elbow angle — chest near floor (bottom)


class PushupAnalyser(BaseExerciseAnalyser):
    def analyse(
        self,
        frames: list[FrameResult],
        frame_count: int,
        fps: float,
    ) -> ExerciseAnalysisResult:
        frame_angles = self._compute_angles(frames)
        rep_count, rep_min_angles = self._count_reps(frame_angles)
        score = self._score(rep_min_angles, frame_angles)
        feedback = self._feedback(rep_count, rep_min_angles, frame_angles)

        return ExerciseAnalysisResult(
            frame_count=frame_count,
            fps=fps,
            exercise_type="pushup",
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

            left_elbow = calculate_angle(pt(_L_SHOULDER), pt(_L_ELBOW), pt(_L_WRIST))
            right_elbow = calculate_angle(pt(_R_SHOULDER), pt(_R_ELBOW), pt(_R_WRIST))

            left_align = calculate_angle(pt(_L_SHOULDER), pt(_L_HIP), pt(_L_ANKLE))
            right_align = calculate_angle(pt(_R_SHOULDER), pt(_R_HIP), pt(_R_ANKLE))

            # Positive hip_offset = hips sagging, negative = hips raised
            l_mid_y = (lms[_L_SHOULDER].y + lms[_L_ANKLE].y) / 2
            r_mid_y = (lms[_R_SHOULDER].y + lms[_R_ANKLE].y) / 2
            hip_offset = round(((lms[_L_HIP].y - l_mid_y) + (lms[_R_HIP].y - r_mid_y)) / 2, 3)

            result.append(FrameAngles(
                frame_index=frame.frame_index,
                angles={
                    "elbow_angle": round((left_elbow + right_elbow) / 2, 1),
                    "alignment_angle": round((left_align + right_align) / 2, 1),
                    "hip_offset": hip_offset,
                },
            ))
        return result

    def _count_reps(
        self, frame_angles: list[FrameAngles]
    ) -> tuple[int, list[float]]:
        reps = 0
        rep_mins: list[float] = []
        in_pushup = False
        current_min = 180.0

        for fa in frame_angles:
            angle = fa.angles["elbow_angle"]
            if not in_pushup and angle < _BOTTOM_THRESHOLD:
                in_pushup = True
                current_min = angle
            elif in_pushup:
                current_min = min(current_min, angle)
                if angle > _TOP_THRESHOLD:
                    reps += 1
                    rep_mins.append(current_min)
                    in_pushup = False
                    current_min = 180.0

        return reps, rep_mins

    def _score(self, rep_min_angles: list[float], frame_angles: list[FrameAngles]) -> int:
        if not rep_min_angles:
            return 0

        avg_min = sum(rep_min_angles) / len(rep_min_angles)
        if avg_min <= 70:
            depth = 80
        elif avg_min <= 80:
            depth = 65
        elif avg_min <= 90:
            depth = 50
        elif avg_min <= 100:
            depth = 35
        else:
            depth = 15

        alignments = [fa.angles["alignment_angle"] for fa in frame_angles]
        avg_align = sum(alignments) / len(alignments)
        align_score = 20 if avg_align >= 160 else max(0, int((avg_align - 120) / 2))

        return min(100, depth + align_score)

    def _feedback(
        self,
        rep_count: int,
        rep_min_angles: list[float],
        frame_angles: list[FrameAngles],
    ) -> list[str]:
        msgs: list[str] = []

        if rep_count == 0:
            msgs.append("No complete repetitions detected — ensure your full body is visible.")
            return msgs

        avg_min = sum(rep_min_angles) / len(rep_min_angles)
        if avg_min <= 80:
            msgs.append("Great depth — chest is getting close to the floor.")
        elif avg_min <= 90:
            msgs.append(f"Good depth — try to go a little lower (avg bottom: {avg_min:.0f}°).")
        else:
            msgs.append(
                f"Go lower — average elbow angle at the bottom was {avg_min:.0f}°. Aim for 80-90°."
            )

        avg_offset = sum(fa.angles["hip_offset"] for fa in frame_angles) / len(frame_angles)
        if avg_offset > 0.05:
            msgs.append("Hips are sagging — engage your core to keep your body in a straight line.")
        elif avg_offset < -0.05:
            msgs.append("Hips are too high — lower them to form a straight line from head to heels.")
        else:
            msgs.append("Good body alignment throughout.")

        return msgs
