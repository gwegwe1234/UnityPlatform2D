using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{

  public GameManager gameManager;
  public AudioClip audioJump;
  public AudioClip audioAttack;
  public AudioClip audioDamaged;
  public AudioClip audioItem;
  public AudioClip audioDie;
  public AudioClip audioFinish;
  public float maxSpeed;
  public float jumpPower;
  Rigidbody2D rigid;
  SpriteRenderer spriteRenderer;
  Animator animator;
  CapsuleCollider2D capsulCollider;
  AudioSource audioSource;

  // Start is called before the first frame update
  void Awake()
  {
    rigid = GetComponent<Rigidbody2D>();
    spriteRenderer = GetComponent<SpriteRenderer>();
    animator = GetComponent<Animator>();
    capsulCollider = GetComponent<CapsuleCollider2D>();
    audioSource = GetComponent<AudioSource>();
  }

  public void PlaySound(string action)
  {
    switch (action)
    {
      case "JUMP":
        audioSource.clip = audioJump;
        break;
      case "ATTACK":
        audioSource.clip = audioAttack;
        break;
      case "DAMAGED":
        audioSource.clip = audioDamaged;
        break;
      case "ITEM":
        audioSource.clip = audioItem;
        break;
      case "DIE":
        audioSource.clip = audioDie;
        break;
      case "FINISH":
        audioSource.clip = audioFinish;
        break;
    }

    audioSource.Play();
  }

  void Update()
  {
    // 단발적인 키 입력은 Update에 넣는게 좋음


    // 점프
    if (Input.GetButtonDown("Jump") && !animator.GetBool("isJumping"))
    {
      rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
      animator.SetBool("isJumping", true);

      PlaySound("JUMP");
    }

    // 멈추는 스피드 조정
    if (Input.GetButtonUp("Horizontal"))
    {
      //rigid.velocity.normalized; // 벡터 크기를 1로 만든 상태 (약간 상대값 구할 떄 쓰는듯./ 말그대로 정규화)
      rigid.velocity = new Vector2(rigid.velocity.normalized.x * 0.5f, rigid.velocity.y);
    }

    // 방향 전환
    if (Input.GetButton("Horizontal"))
    {
      spriteRenderer.flipX = Input.GetAxisRaw("Horizontal") == -1;
    }

    // 애니메이션 전환
    // if (rigid.velocity.normalized.x == 0) { // 멈춤
    //   animator.SetBool("isWalking", false);
    // }
    //
    // if (rigid.velocity.normalized.x != 0) { // 멈춤
    //   animator.SetBool("isWalking", true);
    // }

    if (Mathf.Abs(rigid.velocity.x) < 0.3)
    { // 멈춤
      animator.SetBool("isWalking", false);
    }
    else
    {
      animator.SetBool("isWalking", true);
    }
  }

  // Update is called once per frame
  void FixedUpdate()
  {
    // move speed
    float h = Input.GetAxisRaw("Horizontal");

    rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);

    if (rigid.velocity.x > maxSpeed)
    {
      rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y);
    }
    else if (rigid.velocity.x < maxSpeed * (-1))
    {
      rigid.velocity = new Vector2(maxSpeed * (-1), rigid.velocity.y);
    }

    // Landing platform 땅에 닿을때
    if (rigid.velocity.y < 0)
    {
      Debug.DrawRay(rigid.position, Vector3.down, new Color(0, 1, 0));

      RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector3.down, 1, LayerMask.GetMask("Platform")); // 만든 땅 레이어만 스캔하겠다/

      if (rayHit.collider != null)
      {
        if (rayHit.distance < 0.5f)
        { // 캐릭터가 바닥감지하는 로직
          animator.SetBool("isJumping", false);
        }
      }
    }
  }

  void OnCollisionEnter2D(Collision2D collision)
  {
    if (collision.gameObject.tag == "Enemy")
    {
      // Attack 하늘에서 꽝 찍는거
      if (rigid.velocity.y < 0 && transform.position.y > collision.transform.position.y)
      {
        OnAttack(collision.transform);
      }
      else
      {
        OnDamaged(collision.transform.position);
      }

    }
  }

  void OnAttack(Transform enemy)
  {
    PlaySound("ATTACK");
    gameManager.stagePoint += 100;
    // 점프 모션
    rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
    // Enemy Die
    EnemyMove enemyMove = enemy.GetComponent<EnemyMove>();
    enemyMove.OnDamaged();
  }

  void OnDamaged(Vector2 targetPos)
  {
    // 오디오
    PlaySound("DAMAGED");
    // Health Down
    gameManager.HealthDown();

    // change layer
    gameObject.layer = 11;

    // 흐릿하게 바꿔주는거
    spriteRenderer.color = new Color(1, 1, 1, 0.4f);

    // 튕겨나가는거
    int direction = transform.position.x - targetPos.x > 0 ? 1 : -1;
    rigid.AddForce(new Vector2(direction, 1) * 7, ForceMode2D.Impulse);

    // Animation
    animator.SetTrigger("doDamaged");

    Invoke("OffDamaged", 3);
  }

  void OffDamaged()
  {
    gameObject.layer = 10;

    spriteRenderer.color = new Color(1, 1, 1, 1);
  }

  void OnTriggerEnter2D(Collider2D other)
  {
    if (other.gameObject.tag == "Item")
    {
      bool isBronzeCoin = other.gameObject.name.Contains("Bronze");
      bool isSilverCoin = other.gameObject.name.Contains("Silver");
      bool isGoldCoin = other.gameObject.name.Contains("Gold");

      if (isBronzeCoin)
      {
        gameManager.stagePoint += 50;
      }
      else if (isSilverCoin)
      {
        gameManager.stagePoint += 100;
      }
      else
      {
        gameManager.stagePoint += 300;
      }

      other.gameObject.SetActive(false);

      PlaySound("ITEM");
    }
    else if (other.gameObject.tag == "Flag")
    {
      // Next Stage -> 매니저가 해야됨  
      gameManager.NextStage();

    }
  }

  public void OnDie()
  {
    PlaySound("DIE");
    // 1. Sprite Alpha(색상 변경)
    spriteRenderer.color = new Color(1, 1, 1, 0.4f);
    // 2. Sprite Flip Y
    spriteRenderer.flipY = true;
    // 3. Collider disable
    capsulCollider.enabled = false;
    // 4. Die Effect Jump
    rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
  }

  public void VelocityZero()
  {
    rigid.velocity = Vector2.zero;
  }
}
