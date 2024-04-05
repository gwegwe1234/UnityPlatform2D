using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMove : MonoBehaviour
{
  Rigidbody2D rigid;
  public int nextMove;
  Animator animator;
  SpriteRenderer spriteRenderer;
  CapsuleCollider2D capsulCollider;

  void Awake()
  {
    rigid = GetComponent<Rigidbody2D>();
    animator = GetComponent<Animator>();
    spriteRenderer = GetComponent<SpriteRenderer>();
    capsulCollider = GetComponent<CapsuleCollider2D>();
    Invoke("Think", 5); // 유니티가 제공하는 딜레이 5초뒤 호출
  }

  // Update is called once per frame
  void FixedUpdate()
  {
    // 기본 무빙
    rigid.velocity = new Vector2(nextMove, rigid.velocity.y);

    // 플랫폼 체크
    Vector2 front = new Vector2(rigid.position.x + nextMove * 0.3f, rigid.position.y);
    Debug.DrawRay(front, Vector3.down, new Color(0, 1, 0));

    RaycastHit2D rayHit = Physics2D.Raycast(front, Vector3.down, 1, LayerMask.GetMask("Platform"));

    if (rayHit.collider == null)
    {
      Turn();
    }
  }

  void Think()
  {
    // 다음 활동 세팅
    nextMove = Random.Range(-1, 2);

    // walk speed 세팅
    animator.SetInteger("WalkSpeed", nextMove);

    // flip sprite
    if (nextMove != 0)
    {
      spriteRenderer.flipX = nextMove == 1;
    }

    // recursive
    float nextThinkTime = Random.Range(2f, 5f);
    Invoke("Think", nextThinkTime);
  }

  void Turn()
  {
    nextMove *= -1;
    spriteRenderer.flipX = nextMove == 1;
    CancelInvoke();
    Invoke("Think", 5);
  }

  public void OnDamaged()
  {
    // 몬스터가 죽었을 때 해야할 행동

    // 1. Sprite Alpha(색상 변경)
    spriteRenderer.color = new Color(1, 1, 1, 0.4f);
    // 2. Sprite Flip Y
    spriteRenderer.flipY = true;
    // 3. Collider disable
    capsulCollider.enabled = false;
    // 4. Die Effect Jump
    rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
    // 5. Destroy
    Invoke("DeActive", 1);
  }

  void DeActive()
  {
    gameObject.SetActive(false);
  }
}
