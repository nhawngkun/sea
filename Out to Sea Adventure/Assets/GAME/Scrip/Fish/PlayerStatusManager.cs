using UnityEngine;
using UnityEngine.UI;
using StarterAssets;

public class PlayerStatusManager : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Máu tối đa của người chơi")]
    public float maxHealth = 100f;
    
    [Tooltip("Máu hiện tại của người chơi")]
    public float currentHealth;
    
    [Tooltip("Tốc độ mất máu khi hết oxy")]
    public float healthDecreaseRate = 5f;

    [Header("Oxygen Settings")]
    [Tooltip("Lượng oxy tối đa")]
    public float maxOxygen = 100f;
    
    [Tooltip("Lượng oxy hiện tại")]
    public float currentOxygen;
    
    [Tooltip("Tốc độ giảm oxy khi ở dưới nước")]
    public float oxygenDecreaseRate = 10f;
    
    [Tooltip("Tốc độ hồi phục oxy khi không ở dưới nước")]
    public float oxygenRecoveryRate = 15f;

    [Header("UI Elements")]
    public Image healthBar;
    public Image oxygenBar;
    
    [Tooltip("Hiển thị thanh oxy chỉ khi ở dưới nước")]
    public bool showOxygenOnlyUnderwater = true;

    // Reference to Third Person Controller
    private ThirdPersonController playerController;

    // Start is called before the first frame update
    void Start()
    {
        // Khởi tạo máu và oxy ở mức tối đa
        currentHealth = maxHealth;
        currentOxygen = maxOxygen;
        
        // Lấy tham chiếu đến ThirdPersonController
        playerController = GetComponent<ThirdPersonController>();
        
        // Khởi tạo UI
        UpdateHealthUI();
        UpdateOxygenUI();
        
        // Ẩn thanh oxy nếu cần
        if (showOxygenOnlyUnderwater && oxygenBar != null)
        {
            oxygenBar.transform.parent.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Kiểm tra nếu người chơi ở dưới nước
        if (playerController != null && playerController.IsUnderwater)
        {
            // Hiển thị thanh oxy nếu cần
            if (showOxygenOnlyUnderwater && oxygenBar != null && !oxygenBar.transform.parent.gameObject.activeSelf)
            {
                oxygenBar.transform.parent.gameObject.SetActive(true);
            }
            
            // Giảm oxy khi ở dưới nước
            DecreaseOxygen();
        }
        else
        {
            // Hồi phục oxy khi không ở dưới nước
            RecoverOxygen();
            
            // Ẩn thanh oxy nếu cần
            if (showOxygenOnlyUnderwater && oxygenBar != null && oxygenBar.transform.parent.gameObject.activeSelf)
            {
                oxygenBar.transform.parent.gameObject.SetActive(false);
            }
        }
    }

    // Giảm oxy khi ở dưới nước
    void DecreaseOxygen()
    {
        if (currentOxygen > 0)
        {
            // Giảm oxy theo thời gian
            currentOxygen -= oxygenDecreaseRate * Time.deltaTime;
            currentOxygen = Mathf.Max(0, currentOxygen);
            UpdateOxygenUI();
        }
        else
        {
            // Khi hết oxy, bắt đầu giảm máu
            DecreaseHealth();
        }
    }

    // Hồi phục oxy khi không ở dưới nước
    void RecoverOxygen()
    {
        if (currentOxygen < maxOxygen)
        {
            // Tăng oxy theo thời gian
            currentOxygen += oxygenRecoveryRate * Time.deltaTime;
            currentOxygen = Mathf.Min(maxOxygen, currentOxygen);
            UpdateOxygenUI();
        }
    }

    // Giảm máu khi hết oxy
    void DecreaseHealth()
    {
        currentHealth -= healthDecreaseRate * Time.deltaTime;
        currentHealth = Mathf.Max(0, currentHealth);
        UpdateHealthUI();
        
        // Kiểm tra nếu người chơi đã hết máu
        if (currentHealth <= 0)
        {
            PlayerDeath();
        }
    }

    // Xử lý khi người chơi chết
    void PlayerDeath()
    {
        // Thêm code xử lý khi người chơi chết ở đây
        Debug.Log("Player has died!");
        
        // Ví dụ: Vô hiệu hóa điều khiển
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        // Thêm các hành động khác khi chết (như hiệu ứng, âm thanh, màn hình game over)
    }

    // Cập nhật giao diện thanh máu
    void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = currentHealth / maxHealth;
        }
    }

    // Cập nhật giao diện thanh oxy
    void UpdateOxygenUI()
    {
        if (oxygenBar != null)
        {
            oxygenBar.fillAmount = currentOxygen / maxOxygen;
        }
    }

    // Phương thức public để hồi máu (có thể gọi từ item, checkpoint, etc.)
    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(maxHealth, currentHealth);
        UpdateHealthUI();
    }

    // Phương thức public để nhận sát thương (từ kẻ địch, môi trường, etc.)
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);
        UpdateHealthUI();
        
        if (currentHealth <= 0)
        {
            PlayerDeath();
        }
    }

    // Phương thức public để tăng oxy tối đa (power-up)
    public void IncreaseMaxOxygen(float amount)
    {
        maxOxygen += amount;
        UpdateOxygenUI();
    }
}