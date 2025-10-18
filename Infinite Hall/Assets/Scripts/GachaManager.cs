using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class GachaManager : MonoBehaviour
{
    // ===================================================================
    // I. 유니티 Inspector에서 설정할 공용 변수들
    // ===================================================================

    [Header("Game State")]
    public int currentScore = 0;
    public float chancePoints = 100f;
    public bool isGameActive = false;

    public PlayerMovement playerMovement;

    [Header("Costs and Rewards")]
    public float baseAdvanceCost = 25f;
    public float retreatCost = 5f;
    public float riskBonusCP = 45f;
    public int riskBonusScore = 1;

    [Header("Gacha Setup")]
    public GameObject gachaBoxPrefab;
    public Transform[] boxSpawnLocations;
    public float advanceCostAfterGacha = 0f;

    [Header("Gacha Probabilities (Total 100)")]
    public int prob_Bonus = 15;
    public int prob_Safe = 30;
    public int prob_Chaos = 45;
    public int prob_Risk = 10;

    // UI 텍스트 변수
    [Header("UI Reference")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI cpText;
    public TextMeshProUGUI messageTextCenter;

    // ===================================================================
    // II. 내부 관리 변수 (private)
    // ===================================================================

    private int currentRiskLevel;
    private List<GameObject> activeBoxes = new List<GameObject>();

    // ===================================================================
    // III. Unity 기본 함수
    // ===================================================================

    void Start()
    {
        InitializeGame();
    }

    // ===================================================================
    // IV. 게임 로직 함수
    // ===================================================================

    void InitializeGame()
    {
        currentScore = 0;
        chancePoints = 100f;
        isGameActive = true;

        playerMovement = FindFirstObjectByType<PlayerMovement>();
        if (playerMovement == null) Debug.LogError("PlayerMovement 스크립트를 찾을 수 없습니다.");

        PrepareNewCorridor();

        UpdateUI("Choose Your Box!");
    }

    // 새 복도를 준비하고 가챠 상자를 배치하는 함수
    void PrepareNewCorridor()
    {
        currentRiskLevel = CalculateRiskLevel(currentScore);

        SetupGachaBoxes();

        advanceCostAfterGacha = baseAdvanceCost;

        UpdateUI("Choose Your Box!");
    }

    // 난이도 곡선에 따라 위험 레벨을 계산하는 함수 (로직 유지)
    int CalculateRiskLevel(int score)
    {
        if (score <= 7)
        {
            int roll = Random.Range(1, 101);
            if (roll <= 60) return Random.Range(1, 4);
            if (roll <= 90) return Random.Range(4, 8);
            return Random.Range(8, 11);
        }

        int minLevelIncrease = (currentScore / 10) * 2;
        int minLevel = Mathf.Min(10, 1 + minLevelIncrease);

        int riskRoll = Random.Range(1, 101);
        int finalRisk;

        if (riskRoll <= 30)
            finalRisk = Random.Range(1, 4);
        else if (riskRoll <= 70)
            finalRisk = Random.Range(4, 8);
        else
            finalRisk = Random.Range(8, 11);

        return Mathf.Max(finalRisk, minLevel);
    }

    // 상자 3개를 생성하고 랜덤 힌트를 배정하는 함수
    void SetupGachaBoxes()
    {
        // 1. 기존 상자 제거 (재생성 전 리스트 초기화)
        foreach (GameObject box in activeBoxes)
        {
            Destroy(box);
        }
        activeBoxes.Clear();

        if (playerMovement != null) playerMovement.CanMove(false);

        List<string> boxContents = new List<string> { GetRandomHint(), GetRandomHint(), GetRandomHint() };

        // 2. 상자 생성 및 배정
        for (int i = 0; i < boxSpawnLocations.Length; i++)
        {
            if (boxSpawnLocations.Length > i)
            {
                GameObject newBox = Instantiate(gachaBoxPrefab, boxSpawnLocations[i].position, Quaternion.identity);
                activeBoxes.Add(newBox);

                GachaBoxScript boxScript = newBox.GetComponent<GachaBoxScript>();
                if (boxScript != null)
                {
                    boxScript.SetHint(boxContents[i]);
                }
            }
        }
    }

    // 가챠 확률에 따라 힌트 종류를 반환하는 함수
    string GetRandomHint()
    {
        int roll = Random.Range(1, 101);
        if (roll <= prob_Bonus) return "Bonus";
        if (roll <= prob_Bonus + prob_Safe) return "Safe";
        if (roll <= prob_Bonus + prob_Safe + prob_Chaos) return "Chaos";
        return "Risk";
    }

    // ⭐ 플레이어가 상자를 클릭했을 때 호출될 함수 (지연 처리 추가) ⭐
    public void OnBoxOpened(string hintResult)
    {
        float cost = (hintResult == "Chaos") ? 0f : 1f;
        chancePoints -= cost;

        string message = "ERROR";

        switch (hintResult)
        {
            case "Bonus":
                float bonus = Random.Range(30f, 61f);
                chancePoints += bonus;
                advanceCostAfterGacha = baseAdvanceCost;
                message = $"✨ BONUS! CP +{Mathf.FloorToInt(bonus)} 획득!";
                break;
            case "Safe":
                advanceCostAfterGacha = 5f;
                message = "✅ 안전합니다. 비용 5 CP.";
                break;
            case "Chaos":
                advanceCostAfterGacha = baseAdvanceCost;
                message = "❓ 혼란. 기본 비용 25 CP";
                break;
            case "Risk":
                advanceCostAfterGacha = 45f;
                message = "❌ 위험 감수! 비용 45 CP";
                break;
        }

        // ⭐ 핵심 수정 1: 나머지 상자 제거 명령만 내립니다. ⭐
        foreach (GameObject box in activeBoxes)
        {
            if (box != null)
            {
                Destroy(box);
            }
        }

        // ⭐ 핵심 수정 2: 0.1초 후 리스트를 비우고 플레이어 이동을 허용합니다. (프레임 지연 해결)
        Invoke(nameof(AllowMovement), 0.1f);

        UpdateUI(message);
    }

    // ⭐ 새로 추가된 함수: 지연 후 리스트를 비우고 이동을 허용합니다.
    void AllowMovement()
    {
        activeBoxes.Clear(); // 리스트 정리
        if (playerMovement != null) playerMovement.CanMove(true); // 이동 허용
    }

    // 플레이어가 오른쪽 끝에 도달했을 때 호출될 함수 (전진 시도)
    public void AttemptAdvance()
    {
        if (!isGameActive) return;

        if (playerMovement != null) playerMovement.CanMove(false);

        if (chancePoints < advanceCostAfterGacha)
        {
            GameOver("기회 포인트 부족으로 인해 전진할 수 없습니다.");
            return;
        }

        chancePoints -= advanceCostAfterGacha;

        if (chancePoints > currentRiskLevel)
        {
            bool isRiskSuccess = (advanceCostAfterGacha == 45f);

            currentScore++;

            if (isRiskSuccess)
            {
                chancePoints += riskBonusCP;
                currentScore += riskBonusScore;
                UpdateUI($"🎉 위험 극복! CP +{riskBonusCP} 획득, 스코어 +1!");
            }
            else
            {
                UpdateUI($"전진 성공! 다음 복도 {currentScore}로 이동.");
            }

            // 전진 성공: 상자 재생성 및 위치 초기화
            if (playerMovement != null) playerMovement.ReturnPlayerToCenter();
            PrepareNewCorridor();
        }
        else
        {
            // 전진 실패 시 UI 갱신
            UpdateUI($"전진 실패! ({Mathf.FloorToInt(chancePoints)}CP <= {currentRiskLevel}위험)");
            ReturnToZero();
        }
    }

    // 플레이어가 왼쪽 끝에 도달했을 때 호출될 함수 (후퇴)
    public void Retreat()
    {
        if (!isGameActive || currentScore <= 0) return;

        if (playerMovement != null) playerMovement.CanMove(false);

        if (chancePoints >= retreatCost)
        {
            chancePoints -= retreatCost;
            currentScore = Mathf.Max(0, currentScore - 1);

            // 후퇴 성공 시 UI 갱신
            UpdateUI($"후퇴 성공. CP -{retreatCost}. 현재 스코어 {currentScore} 유지.");

            // 후퇴 성공: 상자 재생성 및 위치 초기화
            if (playerMovement != null) playerMovement.ReturnPlayerToCenter();
            PrepareNewCorridor();
        }
        else
        {
            GameOver("후퇴 비용마저 부족합니다.");
        }
    }

    // 전진 실패 시 0번 복도로 돌아가는 함수
    void ReturnToZero()
    {
        if (playerMovement != null) playerMovement.ReturnPlayerToCenter();
        PrepareNewCorridor();
    }

    // 게임 종료 함수
    void GameOver(string reason)
    {
        isGameActive = false;
        if (playerMovement != null) playerMovement.CanMove(false);
        UpdateUI($"!!! GAME OVER !!! 최종 스코어: {currentScore}");
        Debug.Log($"!!! 게임 오버 !!! 이유: {reason}. 최종 스코어: {currentScore}");
    }

    // UI 업데이트 함수 (렌더링 강제 명령 포함)
    void UpdateUI(string message = "")
    {
        if (scoreText != null)
            scoreText.text = $"SCORE: {currentScore}F";

        if (cpText != null)
            cpText.text = $"CP: {Mathf.FloorToInt(chancePoints)}";

        if (messageTextCenter != null && message != "")
        {
            messageTextCenter.text = message;
        }

        if (!isGameActive && message == "")
        {
            messageTextCenter.text = $"!!! GAME OVER !!!";
        }

        // UI 렌더링 강제 명령 (갱신 지연 해결)
        if (cpText != null && cpText.canvas != null)
        {
            Canvas.ForceUpdateCanvases();
        }
    }
}