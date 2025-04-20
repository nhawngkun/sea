using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpingState : MoveBaseState
{
    private float jumpForce = 7f;
    private enum JumpPhase { JumpStart, JumpMid, JumpEnd }
    private JumpPhase currentPhase = JumpPhase.JumpStart;

    // Timer cho các giai đoạn jumpStart và jumpEnd
    private float phaseTimer = 0f;
    private readonly float jumpStartDuration = 0.4f; // Thời gian của animation khởi động nhảy
    private readonly float jumpEndDuration = 0.4f;   // Thời gian của animation hạ cánh

    // Cờ để theo dõi xem đã đạt đỉnh chưa
    private bool hasReachedPeak = false;

    public override void EnterState(MoveStateManager move)
    {
        // Reset trạng thái nhảy
        currentPhase = JumpPhase.JumpStart;
        phaseTimer = 0f;
        hasReachedPeak = false;

        // Áp dụng lực nhảy ban đầu
        move.velocity.y = jumpForce;

        // Thiết lập animation
        UpdateAnimationState(move);
    }

    public override void UpdetaState(MoveStateManager move)
    {
        // Tăng timer
        phaseTimer += Time.deltaTime;

        // Kiểm tra nếu người chơi đã tiếp xúc với nước - ĐẶT Ở ĐẦU PHƯƠNG THỨC ĐỂ ƯU TIÊN XỬ LÝ
        if (move.IsInWater)
        {
            // Nhảy vào nước và di chuyển --> chuyển sang bơi
            if (move.dir.magnitude > 0.01f)
            {
              
                ExitState(move, move.swimming);
                return;
            }
            // Nhảy vào nước và đứng yên --> chuyển sang nổi
            else
            {
               
                ExitState(move, move.floating);
                return;
            }
        }

        // Xử lý các trạng thái nhảy khác nhau
        switch (currentPhase)
        {
            case JumpPhase.JumpStart:
                HandleJumpStart(move);
                break;

            case JumpPhase.JumpMid:
                HandleJumpMid(move);
                break;

            case JumpPhase.JumpEnd:
                HandleJumpEnd(move);
                break;
        }

        // Cho phép di chuyển ngang khi đang nhảy
        if (Input.GetKey(KeyCode.LeftShift))
        {
            move.currmoveSpeed = move.currrunSpeed * 0.8f; // Hơi chậm hơn khi nhảy
        }
        else
        {
            move.currmoveSpeed = move.currwalkSpeed * 0.8f;
        }
    }

    private void HandleJumpStart(MoveStateManager move)
    {
        // Khi thời gian của animation khởi động nhảy kết thúc, chuyển sang trạng thái JumpMid
        if (phaseTimer >= jumpStartDuration)
        {
            currentPhase = JumpPhase.JumpMid;
            phaseTimer = 0f;
            UpdateAnimationState(move);
        }
    }

    private void HandleJumpMid(MoveStateManager move)
    {
        // Kiểm tra lại nếu đã tiếp xúc với nước khi ở giữa nhảy
        if (move.IsInWater)
        {
            // Nhảy vào nước và di chuyển --> chuyển sang bơi
            if (move.dir.magnitude > 0.01f)
            {
                
                ExitState(move, move.swimming);
                return;
            }
            // Nhảy vào nước và đứng yên --> chuyển sang nổi
            else
            {
               
                ExitState(move, move.floating);
                return;
            }
        }

        // Kiểm tra nếu đã đạt đỉnh nhảy (vận tốc dọc < 0 nghĩa là đang rơi)
        if (move.velocity.y < 0)
        {
            hasReachedPeak = true;
        }

        // Nếu đang rơi và chạm đất
        if (hasReachedPeak && move.IsGround())
        {
            // Đã hạ cánh, chuyển sang trạng thái hạ cánh
            currentPhase = JumpPhase.JumpEnd;
            phaseTimer = 0f;
            UpdateAnimationState(move);
        }
    }

    private void HandleJumpEnd(MoveStateManager move)
    {
        // Khi thời gian của animation hạ cánh kết thúc, chuyển sang trạng thái tương ứng
        if (phaseTimer >= jumpEndDuration)
        {
            // Kiểm tra trạng thái tiếp theo dựa trên input
            if (move.dir.magnitude < 0.1f)
            {
                ExitState(move, move.idle);
            }
            else if (Input.GetKey(KeyCode.LeftShift))
            {
                ExitState(move, move.run);
            }
            else
            {
                ExitState(move, move.Walk);
            }
        }
    }

    private void UpdateAnimationState(MoveStateManager move)
    {
        // Tắt tất cả animation nhảy
        move.ani.SetBool("JumpStart", false);
        move.ani.SetBool("JumpMid", false);
        move.ani.SetBool("JumpEnd", false);
        move.ani.SetBool("Jumping", false);

        // Bật animation tương ứng với giai đoạn hiện tại
        switch (currentPhase)
        {
            case JumpPhase.JumpStart:
                move.ani.SetBool("JumpStart", true);
                break;

            case JumpPhase.JumpMid:
                move.ani.SetBool("JumpMid", true);
                break;

            case JumpPhase.JumpEnd:
                move.ani.SetBool("JumpEnd", true);
                break;
        }
    }

    private void ExitState(MoveStateManager move, MoveBaseState state)
    {
        // Tắt tất cả animation nhảy
        move.ani.SetBool("JumpStart", false);
        move.ani.SetBool("JumpMid", false);
        move.ani.SetBool("JumpEnd", false);
        move.ani.SetBool("Jumping", false);

        // Chuyển sang trạng thái mới
        move.SwitchState(state);
    }
}