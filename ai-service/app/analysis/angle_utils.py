import math


def calculate_angle(
    a: tuple[float, float],
    b: tuple[float, float],
    c: tuple[float, float],
) -> float:
    """Angle in degrees at vertex b, formed by rays b→a and b→c."""
    ba = (a[0] - b[0], a[1] - b[1])
    bc = (c[0] - b[0], c[1] - b[1])
    dot = ba[0] * bc[0] + ba[1] * bc[1]
    mag = math.sqrt(ba[0] ** 2 + ba[1] ** 2) * math.sqrt(bc[0] ** 2 + bc[1] ** 2)
    if mag == 0.0:
        return 0.0
    return math.degrees(math.acos(max(-1.0, min(1.0, dot / mag))))


def torso_lean_angle(
    shoulder: tuple[float, float],
    hip: tuple[float, float],
) -> float:
    """Angle of the spine vector (hip→shoulder) from vertical, in degrees.
    0° = perfectly upright, 90° = horizontal."""
    spine = (shoulder[0] - hip[0], shoulder[1] - hip[1])
    mag = math.sqrt(spine[0] ** 2 + spine[1] ** 2)
    if mag == 0.0:
        return 0.0
    # In MediaPipe coords y=0 is top of frame, so "up" is (0, -1)
    cos_a = max(-1.0, min(1.0, -spine[1] / mag))
    return math.degrees(math.acos(cos_a))
