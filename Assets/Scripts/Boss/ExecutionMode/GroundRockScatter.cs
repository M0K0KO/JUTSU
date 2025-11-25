using UnityEngine;

public class GroundRockScatter : MonoBehaviour
{
    [Header("범위 설정")]
    [Tooltip("흩뿌릴 반경 (미터)")]
    public float scatterRadius = 10f;
    
    
    [Tooltip("Y축(수직) 회전만 줄 것인지 (체크하면 돌이 똑바로 서서 돔)")]
    public bool rotateYOnly = false;

    [Header("지형 밀착 설정 (Raycast)")]
    [Tooltip("체크하면 바닥으로 레이를 쏴서 지형 높이에 맞춤")]
    public bool snapToGround = true;
    
    [Tooltip("레이캐스트가 감지할 레이어 (Ground, Terrain 등)")]
    public LayerMask groundLayer;
    
    [Tooltip("지형 경사에 맞춰 돌을 기울일지 여부")]
    public bool alignToSlope = true;

    [ContextMenu("🪨 땅에 흩뿌리기 (Scatter)")]
    public void ScatterRocks()
    {
        foreach (Transform child in transform)
        {
            // 1. 원형 범위 내 랜덤 위치 계산 (XZ 평면)
            Vector2 randomCircle = Random.insideUnitCircle * scatterRadius;
            Vector3 targetPos = transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

            // 2. 지형 밀착 (Raycast) 로직
            if (snapToGround)
            {
                // 하늘에서 땅으로 레이를 쏨
                Ray ray = new Ray(targetPos + Vector3.up * 50f, Vector3.down);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 100f, groundLayer))
                {
                    // 땅에 닿았으면 그 위치로 설정
                    child.position = hit.point;

                    // 경사면에 맞춰 기울기
                    if (alignToSlope)
                    {
                        child.up = hit.normal;
                    }
                }
                else
                {
                    // 땅을 못 찾았으면 그냥 평지 높이로
                    child.position = targetPos;
                }
            }
            else
            {
                // 평지 모드 (부모 높이 기준)
                child.position = targetPos;
            }

            // 3. 랜덤 회전
            if (rotateYOnly)
            {
                // Y축(제자리) 회전만 랜덤 + 경사면 맞춤 상태 유지
                child.Rotate(Vector3.up, Random.Range(0f, 360f), Space.Self);
            }
            else
            {
                // 모든 축 랜덤 회전 (굴러다니는 돌 느낌)
                child.rotation = Random.rotation;
            }
        }

        Debug.Log("지면 배치 완료!");
    }
}