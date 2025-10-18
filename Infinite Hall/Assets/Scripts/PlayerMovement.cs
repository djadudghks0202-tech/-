using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // === 1. 공용 설정 변수 ===
    public float moveSpeed = 5f;             // 캐릭터 이동 속도
    public float boundaryX = 8f;             // 복도의 좌우 경계선 (Inspector에서 설정)

    // === 2. GachaManager 통신 변수 ===
    private GachaManager gachaManager;
    [SerializeField] private bool canMove = true; // 이동 가능 상태 변수 (Manager가 제어)

    void Start()
    {
        // 최신 권장 함수를 사용하여 GachaManager 스크립트를 찾아 연결합니다.
        gachaManager = FindFirstObjectByType<GachaManager>();

        if (gachaManager == null)
        {
            Debug.LogError("GachaManager 스크립트를 찾을 수 없습니다. GameManager 오브젝트에 연결되었는지 확인하세요.");
        }
    }

    void Update()
    {
        // 게임이 활성화 상태가 아니거나 이동이 불가능하면 움직이지 않습니다.
        if (gachaManager == null || !gachaManager.isGameActive || !canMove) return;

        // 1. 입력 감지
        float horizontalInput = 0f;

        // 키보드 입력 (테스트용)
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            horizontalInput = -1f;
        }
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            horizontalInput = 1f;
        }

        // 모바일 터치 입력 처리
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.position.x < Screen.width / 2)
            {
                horizontalInput = -1f;
            }
            else
            {
                horizontalInput = 1f;
            }
        }

        // 2. 캐릭터 이동
        Vector3 movement = new Vector3(horizontalInput, 0f, 0f);
        transform.position += movement * moveSpeed * Time.deltaTime;

        // 3. 복도 끝 판정 (Trigger 함수가 대신 처리하므로, 여기서는 이동만 수행합니다.)
    }

    //  콜라이더 접촉(Trigger)을 감지하는 Unity 내장 함수 (전진/후퇴 판정)
    private void OnTriggerEnter2D(Collider2D other)
    {
        // GachaManager가 연결되지 않았거나 게임이 비활성화 상태면 무시합니다.
        if (gachaManager == null || !gachaManager.isGameActive) return;

        // 1. 오른쪽 터널(Advance)에 닿았을 때
        if (other.CompareTag("Advance"))
        {
            // 닿자마자 캐릭터를 터널 중앙에 고정하고 함수 호출
            transform.position = new Vector3(boundaryX, transform.position.y, transform.position.z);
            gachaManager.AttemptAdvance();
        }
        // 2. 왼쪽 터널(Retreat)에 닿았을 때
        else if (other.CompareTag("Retreat"))
        {
            // 닿자마자 캐릭터를 터널 중앙에 고정하고 함수 호출
            transform.position = new Vector3(-boundaryX, transform.position.y, transform.position.z);
            gachaManager.Retreat();
        }
    }

    // GachaManager가 이 함수를 호출하여 캐릭터 이동을 제어합니다.
    public void CanMove(bool state)
    {
        canMove = state;
    }

    // GachaManager가 이 함수를 호출하여 캐릭터를 중앙으로 즉시 이동시킵니다.
    public void ReturnPlayerToCenter()
    {
        transform.position = Vector3.zero; // X=0, Y=0, Z=0 위치로 이동
    }

    // 참고: 기존의 CheckBoundaryAndCallGameLogic() 함수는 OnTriggerEnter2D로 대체되었습니다.
}