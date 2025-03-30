using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;


// Quản lý loại và hành vi của cá
public class FishManager : MonoBehaviour
{
    [System.Serializable]
    public class FishType
    {
        public string name;
        public float minWeight;
        public float maxWeight;
        public float catchDifficulty;
        public Sprite fishImage;
        public GameObject fishPrefab;
    }

    public List<FishType> fishTypes = new List<FishType>();
    public Transform fishSpawnArea;

    // Sinh ra các loại cá trong khu vực
    public void SpawnFish(int count)
    {
        for (int i = 0; i < count; i++)
        {
            FishType randomFishType = fishTypes[Random.Range(0, fishTypes.Count)];
            Vector3 randomPosition = GetRandomSpawnPosition();
            
            GameObject fishObject = Instantiate(randomFishType.fishPrefab, randomPosition, Quaternion.identity);
            Fish fishComponent = fishObject.GetComponent<Fish>();
            
            if (fishComponent != null)
            {
                fishComponent.Initialize(
                    randomFishType.name, 
                    Random.Range(randomFishType.minWeight, randomFishType.maxWeight),
                    randomFishType.catchDifficulty
                );
            }
        }
    }

    // Lấy vị trí ngẫu nhiên để sinh cá
    private Vector3 GetRandomSpawnPosition()
    {
        if (fishSpawnArea == null) return Vector3.zero;

        Bounds spawnBounds = new Bounds(fishSpawnArea.position, fishSpawnArea.localScale);
        return new Vector3(
            Random.Range(spawnBounds.min.x, spawnBounds.max.x),
            fishSpawnArea.position.y,
            Random.Range(spawnBounds.min.z, spawnBounds.max.z)
        );
    }
}