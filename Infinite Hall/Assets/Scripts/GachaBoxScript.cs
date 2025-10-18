using UnityEngine;

public class GachaBoxScript : MonoBehaviour
{
    // 이 변수에 이 상자가 'Safe', 'Risk' 등 어떤 힌트를 가지고 있는지 저장됩니다.
    private string myHintResult;

    // GachaManager를 참조하여 결과를 알려주기 위한 변수
    private GachaManager manager;

    void Start()
    {
        // 최신 권장 함수를 사용하여 GachaManager를 찾아 연결합니다.
        manager = FindFirstObjectByType<GachaManager>();
    }

    // GachaManager가 이 상자를 생성할 때 호출하여 힌트를 설정하는 함수
    public void SetHint(string hint)
    {
        myHintResult = hint;
    }

    // 마우스/터치로 상자를 클릭했을 때 호출되는 Unity 내장 함수
    private void OnMouseDown()
    {
        if (manager != null && manager.isGameActive)
        {
            // 1. GachaManager에게 이 상자의 힌트 결과를 알립니다.
            manager.OnBoxOpened(myHintResult);

            // 2. ⭐ 핵심 수정: 상자 결과를 알린 후, 클릭된 이 상자(자기 자신)를 즉시 파괴합니다. ⭐
            Destroy(gameObject);
        }
    }
}
