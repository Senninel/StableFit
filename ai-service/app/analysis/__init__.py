from app.analysis.base_analyser import BaseExerciseAnalyser
from app.analysis.bicep_curl_analyser import BicepCurlAnalyser
from app.analysis.lunge_analyser import LungeAnalyser
from app.analysis.plank_analyser import PlankAnalyser
from app.analysis.pullup_analyser import PullupAnalyser
from app.analysis.pushup_analyser import PushupAnalyser
from app.analysis.squat_analyser import SquatAnalyser

ANALYSERS: dict[str, BaseExerciseAnalyser] = {
    "squat": SquatAnalyser(),
    "pushup": PushupAnalyser(),
    "plank": PlankAnalyser(),
    "bicep_curl": BicepCurlAnalyser(),
    "lunge": LungeAnalyser(),
    "pullup": PullupAnalyser(),
}
