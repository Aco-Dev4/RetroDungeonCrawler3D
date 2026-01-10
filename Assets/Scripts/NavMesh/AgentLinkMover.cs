using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public enum OffMeshLinkMoveMethod {
   Teleport,
   NormalSpeed,
   Parabola,
   Curve
}

[RequireComponent (typeof (UnityEngine.AI.NavMeshAgent))]
public class AgentLinkMover : MonoBehaviour {
   public OffMeshLinkMoveMethod method = OffMeshLinkMoveMethod.Parabola;
   public AnimationCurve curve = new AnimationCurve ();
   IEnumerator Start () {
     UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent> ();
     agent.autoTraverseOffMeshLink = false;
     while (true) {
       if (agent.isOnOffMeshLink) {
         if (method == OffMeshLinkMoveMethod.NormalSpeed)
           yield return StartCoroutine (NormalSpeed (agent));
         else if (method == OffMeshLinkMoveMethod.Parabola)
           yield return StartCoroutine (Parabola (agent, 2.0f, 0.5f));
         else if (method == OffMeshLinkMoveMethod.Curve)
           yield return StartCoroutine (Curve (agent, 0.5f));
         agent.CompleteOffMeshLink ();
       }
       yield return null;
     }
   }
   IEnumerator NormalSpeed (UnityEngine.AI.NavMeshAgent agent) {
     UnityEngine.AI.OffMeshLinkData data = agent.currentOffMeshLinkData;
     Vector3 endPos = data.endPos + Vector3.up*agent.baseOffset;
     while (agent.transform.position != endPos) {
       agent.transform.position = Vector3.MoveTowards (agent.transform.position, endPos, agent.speed*Time.deltaTime);
       yield return null;
     }
   }
   IEnumerator Parabola (UnityEngine.AI.NavMeshAgent agent, float height, float duration) {
     UnityEngine.AI.OffMeshLinkData data = agent.currentOffMeshLinkData;
     Vector3 startPos = agent.transform.position;
     Vector3 endPos = data.endPos + Vector3.up*agent.baseOffset;
     float normalizedTime = 0.0f;
     while (normalizedTime < 1.0f) {
       float yOffset = height * 4.0f*(normalizedTime - normalizedTime*normalizedTime);
       agent.transform.position = Vector3.Lerp (startPos, endPos, normalizedTime) + yOffset * Vector3.up;
       normalizedTime += Time.deltaTime / duration;
       yield return null;
     }
   }
   IEnumerator Curve (UnityEngine.AI.NavMeshAgent agent, float duration) {
     UnityEngine.AI.OffMeshLinkData data = agent.currentOffMeshLinkData;
     Vector3 startPos = agent.transform.position;
     Vector3 endPos = data.endPos + Vector3.up*agent.baseOffset;
     float normalizedTime = 0.0f;
     while (normalizedTime < 1.0f) {
       float yOffset = curve.Evaluate (normalizedTime);
       agent.transform.position = Vector3.Lerp (startPos, endPos, normalizedTime) + yOffset * Vector3.up;
       normalizedTime += Time.deltaTime / duration;
       yield return null;
     }
   }
}
