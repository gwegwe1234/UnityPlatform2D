using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

// 점수와 스테이지 관리를 함
public class GameManager : MonoBehaviour
{
    public int totalPoint;
    public int stagePoint;
    public int stageIndex;
    public int health;
    public PlayerMove player;
    public GameObject[] Stages;
    public Image[] UIhealth;
    public Text UIPoint;
    public Text UIStage;
    public GameObject UIRestartButton;

    // Start is called before the first frame update
    public void NextStage()
    {
        //Change Stage
        if (stageIndex < Stages.Length - 1)
        {
            Stages[stageIndex].SetActive(false);
            stageIndex++;
            Stages[stageIndex].SetActive(true);
            PlayerReposition();

            UIStage.text = "STAGE " + (stageIndex + 1);
        }
        else
        { // GAME CLEAR
            Time.timeScale = 0;

            // Result UI
            Debug.Log("게임 클리어!");

            // Restart Button UI
            Text btnText = UIRestartButton.GetComponentInChildren<Text>();
            btnText.text = "Game Clear!";
            UIRestartButton.SetActive(true);
        }

        player.PlaySound("FINISH");
        //Calculate Point
        totalPoint += stagePoint;
        stagePoint = 0;
    }

    void Update() 
    {
        UIPoint.text = (totalPoint + stagePoint).ToString();
    }

    public void HealthDown()
    {
        if (health > 1)
        {
            health--;
            UIhealth[health].color = new Color(1, 0, 0, 0.2f);
        }
        else
        {
            health--;
            // All Health Ui Off
            UIhealth[health].color = new Color(1, 0, 0, 0.2f);

            // PlayerDieEffect
            player.OnDie();

            // Result Ui
            Debug.Log("죽었습니다.");

            // Retry Button UI
            UIRestartButton.SetActive(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {

            if (health > 1)
            {
                // Player Reposition
                // other.attachedRigidbody.velocity = Vector2.zero;
                // other.transform.position = new Vector3(-7.84f , -2.79f ,-1);
                PlayerReposition();
            }

            HealthDown();
        }
    }

    void PlayerReposition()
    {
        player.transform.position = new Vector3(-7.84f, -2.79f, -1);
        player.VelocityZero();
    }
    
    public void Restart() {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }
}
