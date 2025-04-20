using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishController : MonoBehaviour
{
    [Header("Các Bộ Phận Của Cá Mập")]
    [Tooltip("Transform của vi ngực trái")]
    public Transform viNgucTrai;

    [Tooltip("Transform của vi ngực phải")]
    public Transform viNgucPhai;

    [Tooltip("Transform của vi lưng")]
    public Transform viLung;

    [Tooltip("Transform của vi lưng dưới")]
    public Transform viLungDuoi;

    [Tooltip("Transform của hàm dưới (nếu có)")]
    public Transform hamDuoi;



    [Header("Thiết Lập Hoạt Ảnh")]
    [Tooltip("Tốc độ vẫy vi ngực")]
    public float tocDoVayViNguc = 0.9f;

    [Tooltip("Biên độ vẫy vi ngực")]
    public float bienDoViNguc = 15.0f;

    [Header("Các Đặc Tính Hành Vi")]

    [Tooltip("Xác suất cá mập mở miệng (0-1)")]
    [Range(0f, 1f)]
    public float xacSuatMoMieng = 0.1f;

    [Tooltip("Thời gian mở miệng tối đa")]
    public float thoiGianMoMiengToiDa = 2.0f;

    // Biến riêng tư
    private float thoiGianDem = 0f;
    private bool dangMoMieng = false;
    private float thoiGianMoMieng = 0f;
    private float gocMoMieng = 0f;

    void Start()
    {
        // Khởi tạo
    }

    void Update()
    {
        // Cập nhật bộ đếm thời gian
        thoiGianDem += Time.deltaTime;

        // Cập nhật hoạt ảnh cho các bộ phận
        HoatAnhCacBoPhan();

        // Quản lý hành vi mở miệng
        QuanLyMoMieng();
    }



    void HoatAnhCacBoPhan()
    {
        // Tính giá trị sin cho hoạt ảnh
        float sinValue = Mathf.Sin(thoiGianDem * tocDoVayViNguc);

        // Hoạt ảnh vẫy vi ngực - nhẹ nhàng hơn
        float sinViNguc = Mathf.Sin(thoiGianDem * tocDoVayViNguc);

        // Vi ngực trái
        if (viNgucTrai != null)
            viNgucTrai.localRotation = Quaternion.Euler(
                0,
                0,
                sinViNguc * bienDoViNguc - bienDoViNguc / 2); // Tạo góc nghiêng lên xuống

        // Vi ngực phải
        if (viNgucPhai != null)
            viNgucPhai.localRotation = Quaternion.Euler(
                0,
                0,
                -sinViNguc * bienDoViNguc + bienDoViNguc / 2); // Ngược với vi trái

        // Vi lưng nhẹ nhàng uyển chuyển theo chuyển động cơ thể
        if (viLung != null)
            viLung.localRotation = Quaternion.Euler(0, sinValue * bienDoViNguc * 0.3f, 0);

        // Vi lưng dưới chuyển động tương tự vi lưng nhưng với độ trễ
        if (viLungDuoi != null)
            viLungDuoi.localRotation = Quaternion.Euler(0, sinValue * bienDoViNguc * 0.25f, 0);
    }

    void QuanLyMoMieng()
    {
        if (hamDuoi != null)
        {
            // Nếu đang mở miệng
            if (dangMoMieng)
            {
                thoiGianMoMieng -= Time.deltaTime;

                // Nếu đã hết thời gian mở miệng
                if (thoiGianMoMieng <= 0)
                {
                    dangMoMieng = false;
                }
                else
                {
                    // Nếu sắp đóng miệng, giảm dần góc
                    if (thoiGianMoMieng < 0.5f)
                    {
                        gocMoMieng = Mathf.Lerp(0, gocMoMieng, thoiGianMoMieng * 2);
                    }
                }
            }
            // Nếu đang không mở miệng, có cơ hội ngẫu nhiên để mở
            else if (Random.value < xacSuatMoMieng * Time.deltaTime)
            {
                dangMoMieng = true;
                thoiGianMoMieng = Random.Range(0.5f, thoiGianMoMiengToiDa);
                gocMoMieng = Random.Range(10f, 25f);
            }

            // Cập nhật xoay hàm dưới
            hamDuoi.localRotation = Quaternion.Euler(-gocMoMieng, 0, 0);
        }
    }

    // Phương thức để dùng trong Animator hoặc kích hoạt từ bên ngoài
    public void TaoCuTanCong()
    {
        StartCoroutine(HoatAnhTanCong());
    }

    IEnumerator HoatAnhTanCong()
    {
        // Mở miệng rộng
        if (hamDuoi != null)
        {
            dangMoMieng = true;
            gocMoMieng = 30f;
            thoiGianMoMieng = 1.5f;
        }

        // Duy trì trong 1.5 giây
        yield return new WaitForSeconds(1.5f);
    }
}