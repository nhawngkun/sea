using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FishingSystem : MonoBehaviour
{
    public Camera mainCamera;
    public Transform hookPoint;
    public float maxCastDistance = 50f;
    public LayerMask fishLayer;

    [Header("UI References")]
    public Text fishCaughtText;
    public Text inventoryWeightText;

    public List<Fish> caughtFishes = new List<Fish>();
    private bool isCasting = false;
    private Vector3 castTarget;

    // Thực hiện ném câu
    public void Cast()
    {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxCastDistance))
        {
            isCasting = true;
            castTarget = hit.point;
            StartCoroutine(MovehookToTarget());
        }
    }

    // Di chuyển móc câu tới điểm đích
    private IEnumerator MovehookToTarget()
    {
        float journeyLength = Vector3.Distance(hookPoint.position, castTarget);
        float startTime = Time.time;
        float distanceCovered = 0f;

        while (distanceCovered < journeyLength)
        {
            float fractionOfJourney = distanceCovered / journeyLength;
            hookPoint.position = Vector3.Lerp(hookPoint.position, castTarget, fractionOfJourney);
            
            distanceCovered = (Time.time - startTime) * 10f;
            
            // Kiểm tra va chạm với cá
            Collider[] hitFishes = Physics.OverlapSphere(hookPoint.position, 1f, fishLayer);
            if (hitFishes.Length > 0)
            {
                TryCatchFish(hitFishes[0].GetComponent<Fish>());
                break;
            }

            yield return null;
        }

        StartCoroutine(ReelBack());
    }

    // Kéo móc câu về
    private IEnumerator ReelBack()
    {
        float journeyLength = Vector3.Distance(hookPoint.position, transform.position);
        float startTime = Time.time;
        float distanceCovered = 0f;

        while (distanceCovered < journeyLength)
        {
            float fractionOfJourney = distanceCovered / journeyLength;
            hookPoint.position = Vector3.Lerp(hookPoint.position, transform.position, fractionOfJourney);
            
            distanceCovered = (Time.time - startTime) * 10f;
            yield return null;
        }

        isCasting = false;
        UpdateUI();
    }

    // Thử bắt cá
    private void TryCatchFish(Fish fish)
    {
        if (fish != null)
        {
            float catchChance = Random.Range(0f, 1f);
            if (catchChance > fish.catchDifficulty)
            {
                caughtFishes.Add(fish);
                fish.gameObject.SetActive(false);
            }
        }
    }

    // Cập nhật giao diện
    private void UpdateUI()
    {
        if (fishCaughtText != null)
            fishCaughtText.text = $"Cá đã bắt: {caughtFishes.Count}";

        if (inventoryWeightText != null)
        {
            float totalWeight = 0;
            foreach (Fish fish in caughtFishes)
            {
                totalWeight += fish.weight;
            }
            inventoryWeightText.text = $"Tổng trọng lượng: {totalWeight:F2} kg";
        }
    }

    // Bán cá
    public void SellFish()
    {
        float totalMoney = 0;
        foreach (Fish fish in caughtFishes)
        {
            // Tính tiền theo trọng lượng và loại cá
            totalMoney += fish.weight * 10;
        }

        Debug.Log($"Đã bán được {totalMoney:F2} đồng");
        caughtFishes.Clear();
        UpdateUI();
    }
}
