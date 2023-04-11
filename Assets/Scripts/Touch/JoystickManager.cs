using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/*******************************************
   
    캐릭터 조종 관련
  * 조이스틱
  * 버튼(스킬, 캐릭터 변경, 카메라 고정, 위치 고정, 집결)
  * 
*******************************************/

public class JoystickManager : MonoBehaviour { 

    //UI 상 위치와 월드 포지션 호환을 위한 캔버스
    public Canvas parentCanvas;

    //조이스틱과 관련된 내용
    //****************************************
    public RectTransform rightPivot;     //오른쪽 네모, joystick과의 거리를 계산해서 그걸 이동 한계로 설정
    public RectTransform joystick;       //조이스틱
    Vector2 touchPos;                    //터치한 위치
    Vector2 joystickFirstPos;            //조이스틱 초기 위치, 이 위치와 터치 위치로 현재 방향 판단
    Vector2 joyVec;                      //조이스틱 방향 벡터(Normalized Vector, y, z 성분은 0)
    float Radius;                        //조이스틱 이동 한계, Start 함수에서 계산
    //****************************************

    //캐릭터와 현재 조종 중인 캐릭터 정보
    //****************************************
    Character controlChar;                //지금 조종 중인 캐릭터, 기본적으로 Chars[0]로 설정
    Character[] chars = new Character[3]; //캐릭터들, GameManager의 AssignList 불러옴
    public int nowChar = 0;               //지금 조종 중인 캐릭터의 Index
    public GameObject arrow;              //지금 조종 중인 캐릭터 위에 떠있는 화살표
    //****************************************

    //캐릭터 이동 코루틴 관련
    //****************************************
    public bool isCharMove;               //코루틴 중첩 방지
    public Coroutine charMoveCoroutine;   //이동 코루틴
    //****************************************


    //캐릭터 공격 코루틴 관련
    //****************************************
    public bool[] isCoroutineOn = new bool[3];              //코루틴 중첩 방지
    public Coroutine[] skillCoroutines = new Coroutine[3];  //공격 코루틴
    //****************************************
    
    public PinchAndSwipe swipeManager;

    //버튼들
    //인스펙터 창에서 이미 할당됨
    //****************************************
    public Button BasicAttackButton;      //기본공격 버튼
    public Button Skill1Button;           //스킬 1 버튼
    public Button Skill2Button;           //스킬 2 버튼
    public Button CCButton1;              //캐릭터 변경 버튼 1번
    public Button CCButton2;              //캐릭터 변경 버튼 2번
    public Button HoldButton;             //위치 고정 버튼
    //****************************************

    //버튼 이미지 관련 (0 : Hero, 1 : Chaser, 2 : Crusher)
    //인스펙터 창에서 이미 할당됨
    //****************************************
    public Sprite[] basicAttackImages = new Sprite[3];  //기본 공격 버튼 이미지
    public Sprite chaserClawAttackImage;                //추적자 클로 공격 버튼 이미지

    public Sprite[] skill1Images = new Sprite[3];       //스킬 1 버튼 이미지
    public Sprite[] skill2Images = new Sprite[3];       //스킬 2 버튼 이미지
    public Sprite chaserClawSkill2Image;                //추적자 클로 쇄도 버튼 이미지

    public Sprite[] charChangeImages;                   //캐릭터 변경 이미지
                                                        //(0 : Dummy, 1 : Hero, 2 : Chaser, 3: Crusher)

    public Sprite[] holdImages = new Sprite[2];         //홀드 버튼 이미지(0 : 홀드 취소, 1 : 홀드)
    //****************************************

    //스킬 버튼 테두리 관련, 쿨타임 돌 때는 안보이게 함
    //****************************************
    public Image[] SkillEdge = new Image[2];
    //****************************************
    
    //캐릭터 변환 중 또 캐릭터 변환 하기 방지를 위한 bool
    //****************************************
    bool isChanging;
    //****************************************
    
    //카메라 고정 bool
    //****************************************
    bool cameraHold;
    //****************************************

    //더미 캐릭터 프리팹
    //****************************************
    public GameObject DummyCharacter;
    //****************************************

    //집결 관련
    //****************************************
    public bool isGather;               //집결 버튼 누른 상태
    public Image[] joystickSprites;     //집결 버튼 눌렀을 시 조이스틱 숨기기 용
    //****************************************
    
    private void Start()
    {
        arrow.SetActive(false);

        //조이스틱 위치 조정
        //*****************************************
        //초기 위치 저장
        joystickFirstPos = joystick.anchoredPosition;
        //조이스틱 이동 한계 계산
        Radius = Vector3.Distance(joystickFirstPos, rightPivot.anchoredPosition);
        //*****************************************

    }

    //NightManager의 Start 함수에서 호출, 캐릭터 정보 불러오기
    public void LoadCharData(Character[] c)
    {
        for (int i = 0; i < 3; i++)
            chars[i] = c[i];

        controlChar = chars[0];

        //이미지 설정
        SetButtonColor();
    }

    //터치 조작에 따른 조이스틱 및 캐릭터 이동
    //집결 처리
    private void Update()
    {
        //집결 버튼 누른 상태
        if (isGather)
        {
            //안드로이드
            if(Application.platform == RuntimePlatform.Android)
            {
                if(Input.touchCount == 1)
                {
                    Touch touch = Input.GetTouch(0);
                    if(touch.phase == TouchPhase.Began)
                    {
                        Time.timeScale = 1;
                        //집결 터치 위치 받아오기
                        Vector3 gatherPos = Camera.main.ScreenToWorldPoint(touch.position);

                        isGather = false;
                        for (int i = 0; i < 3; i++)
                        {
                            joystickSprites[i].enabled = true;
                            if (chars[i].isActiveAndEnabled && chars[i].CanMove())
                            {
                                chars[i].StopGather();
                                chars[i].Gather(gatherPos);
                            }
                        }
                    }
                }
            }
            //컴퓨터
            else
            {
                if(Input.GetMouseButtonDown(0))
                {
                    Time.timeScale = 1;
                    //집결 터치 위치 받아오기
                    Vector3 gatherPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                    isGather = false;
                    for (int i = 0; i < 3; i++)
                    {
                        joystickSprites[i].enabled = true;
                        if (chars[i].isActiveAndEnabled && !chars[i].isHold)
                        {
                            chars[i].StopGather();
                            chars[i].Gather(gatherPos);
                        }
                    }
                }
            }
        }
        //집결 버튼 X -> 이동 관련
        else
        {
            //안드로이드
            if (Application.platform == RuntimePlatform.Android)
            {
                if (Input.touchCount > 0)
                {
                    for (int i = 0; i < Input.touchCount; i++)
                    {
                        Vector3 Pos = Input.GetTouch(i).position;

                        //movePos로 현재 터치 위치 받아오기
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvas.transform as RectTransform, Pos, parentCanvas.worldCamera, out touchPos);

                        //조이스틱 터치 영역 터치 시
                        if (Mathf.Abs(touchPos.x - joystickFirstPos.x) < Radius + rightPivot.sizeDelta.x * 0.8f)
                        {
                            if (Mathf.Abs(touchPos.y - joystickFirstPos.y) < rightPivot.sizeDelta.y * 0.6f)
                            {
                                if (controlChar.isGather)
                                    controlChar.StopGather();

                                joyVec = new Vector3((touchPos.x - joystickFirstPos.x), 0).normalized;
                                controlChar.moveVector = joyVec;
                                float Dis = Mathf.Abs(touchPos.x - joystickFirstPos.x);

                                if (Dis < Radius)
                                    joystick.anchoredPosition = joystickFirstPos + joyVec * Dis;
                                else
                                    joystick.anchoredPosition = joystickFirstPos + joyVec * Radius;

                                break;
                            }
                        }
                        else
                        {
                            joystick.anchoredPosition = joystickFirstPos;
                            controlChar.moveVector = joyVec = Vector2.zero;
                        }
                    }
                }
                else
                {
                    joystick.anchoredPosition = joystickFirstPos;
                    controlChar.moveVector = joyVec = Vector2.zero;
                }
            }
            //컴퓨터, 이 부분은 테스트용
            else
            {
                if (Input.GetMouseButton(0))
                {
                    Vector3 Pos = Input.mousePosition;

                    //movePos로 현재 터치 위치 받아오기
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvas.transform as RectTransform, Pos, parentCanvas.worldCamera, out touchPos);

                    //조이스틱 터치 영역 터치 시
                    if (Mathf.Abs(touchPos.x - joystickFirstPos.x) < Radius + rightPivot.sizeDelta.x * 0.5f)
                    {
                        if (Mathf.Abs(touchPos.y - joystickFirstPos.y) < rightPivot.sizeDelta.y * 0.8f)
                        {
                            if (controlChar.isGather)
                                controlChar.StopGather();

                            joyVec = new Vector3((touchPos.x - joystickFirstPos.x), 0).normalized;
                            controlChar.moveVector = joyVec;
                            float Dis = Mathf.Abs(touchPos.x - joystickFirstPos.x);

                            if (Dis < Radius)
                                joystick.anchoredPosition = joystickFirstPos + joyVec * Dis;
                            else
                                joystick.anchoredPosition = joystickFirstPos + joyVec * Radius;
                        }
                    }
                }

                if (Input.GetMouseButtonUp(0))
                {
                    joystick.anchoredPosition = joystickFirstPos;
                    controlChar.moveVector = joyVec = Vector2.zero;
                }
            }
        }
    }

    //NightManager의 StartBattle 함수에서 호출, CharMoveCoroutine에 저장
    //캐릭터 이동, 애니메이션 적용
    public IEnumerator ControlCharMove()
    {
        controlChar.isControl = true;

        //코루틴 중첩 방지
        if (!isCharMove)
        {
            isCharMove = true;
            yield return new WaitForSeconds(0.03f);
            StartCoroutine(CharChange());

            while (true)
            {
                SkillBtnCooldownSet();
                arrow.transform.position = controlChar.arrowPos.transform.position;

                //공격 또는 위치사수 중이 아닐 때만 이동
                if (!controlChar.isAttack && controlChar.CanMove())
                {
                    //오른쪽 이동
                    if (joyVec.x > 0)
                    {
                        controlChar.Move(Vector3.right);

                        //애니메이션 적용, 휠윈드나 점프 시에는 무시
                        if (!(controlChar.isWhirlWind || controlChar.isJumping))
                        {
                            controlChar.AnimationSet(Anim.Move);
                        }
                    }
                    //왼쪽 이동
                    else if (joyVec.x < 0)
                    {
                        controlChar.Move(Vector3.left);

                        //애니메이션 적용, 휠윈드나 점프 시에는 무시
                        if (!(controlChar.isWhirlWind || controlChar.isJumping))
                        {
                            controlChar.AnimationSet(Anim.Move);
                        }
                    }
                    //이동도 공격도 안함 -> Idle 에니메이션 적용
                    else if (!controlChar.isAttack && !controlChar.isGather)
                    {
                        //휠윈드나 점프 시에는 무시
                        if (!(controlChar.isWhirlWind || controlChar.isJumping))
                        {
                            controlChar.AnimationSet(Anim.Idle);
                        }
                    }
                }

                yield return new WaitForSeconds(0.03f);
            }
        }
    }

    //ControlCharMove 함수 맨 앞에서 호출 
    //지금 조종 중인 캐릭터가 죽으면 다른 캐릭터로 자동 변경
    private IEnumerator CharChange()
    {
        while (true)
        {
            //지금 조종 중인 캐릭터가 죽었을 때 다른 캐릭터로 변경
            if (!controlChar.isActiveAndEnabled)
            {
                arrow.SetActive(false);
                CharChangeMain();
            }
            else
            {
                arrow.SetActive(true);
            }

            yield return new WaitForSeconds(0.4f);
        }
    }
    
    //Basic Attack Button 관련
    //******************************************
    //공격 버튼 터치 시 호출, Coroutine_Basic 호출
    public void Btn_BasicIn()
    {
        controlChar.StopGather();
        skillCoroutines[0] = StartCoroutine(Coroutine_Basic());
    }

    //공격 버튼 땔 시 호출, Coroutine_Basic 정지
    public void Btn_BasicOut()
    {
        if(isCoroutineOn[0])
        {
            isCoroutineOn[0] = false;

            if (skillCoroutines[0] != null)
                StopCoroutine(skillCoroutines[0]);

            if (controlChar.charClass == CharClass.Astronomer)
            {
                controlChar.ChargeEnd();
            }
        }
    }

    //주기적으로 공격 명령, 공격 버튼에서 손을 떼면 정지
    IEnumerator Coroutine_Basic()
    {
        if(!isCoroutineOn[0])
        {
            isCoroutineOn[0] = true;

            while (isCoroutineOn[0])
            {
                controlChar.AttackBtn();

                yield return new WaitForSeconds(0.1f);
            }
        }
    }
    //******************************************

    //Skill 1 Button 관련
    //******************************************
    //Skill 1 버튼 터치 시 호출, Coroutine_Skill1 호출
    public void Btn_Skill1In()
    {
        controlChar.StopGather();
        skillCoroutines[1] = StartCoroutine(Coroutine_Skill1());
    }

    //Skill 1 버튼 땔 시 호출, Coroutine_Skill1 정지
    public void Btn_Skill1Out()
    {
        if (isCoroutineOn[1])
        {
            isCoroutineOn[1] = false;

            if (skillCoroutines[1] != null)
                StopCoroutine(skillCoroutines[1]);

            if (controlChar.charClass == CharClass.Astronomer)
            {
                controlChar.ChargeEnd();
            }
            
        }
    }

    //주기적으로 Skill 1 명령, 버튼에서 손을 떼면 정지
    IEnumerator Coroutine_Skill1()
    {
        if(!isCoroutineOn[1])
        {
            isCoroutineOn[1] = true;

            while (isCoroutineOn[1])
            {
                controlChar.Skill1();

                yield return new WaitForSeconds(0.1f);
            }
        }
    }
    //******************************************

    //Skill 2 Button 관련
    //******************************************
    //Skill 2 버튼 터치 시 호출, Coroutine_Skill2 호출
    public void Btn_Skill2In()
    {
        controlChar.StopGather();
        skillCoroutines[2] = StartCoroutine(Coroutine_Skill2());
    }

    //Skill 2 버튼 땔 시 호출, Coroutine_Skill2 정지
    public void Btn_Skill2Out()
    {
        if (isCoroutineOn[2])
        {
            isCoroutineOn[2] = false;

            if (skillCoroutines[2] != null)
                StopCoroutine(skillCoroutines[2]);

            if (controlChar.charClass == CharClass.Astronomer)
            {
                controlChar.MilkyWayEnd();
            }
            
        }
    }

    //주기적으로 Skill 2 명령, 버튼에서 손을 떼면 정지
    IEnumerator Coroutine_Skill2()
    {
        if (!isCoroutineOn[2])
        {
            isCoroutineOn[2] = true;

            while (isCoroutineOn[2])
            {
                controlChar.Skill2();

                if (controlChar.charClass == CharClass.Astronomer && !controlChar.isMilkyway)
                {
                    isCoroutineOn[2] = false;
                }

                yield return new WaitForSeconds(0.1f);
            }
        }
    }
    //******************************************

    //지금 조종 중인 캐릭터에게 점프 명령
    public void Btn_Jump()
    {
        if (controlChar.isActiveAndEnabled)
        {
            controlChar.StopGather();
            controlChar.Jump();
        }
    }

    //지금 조종 중인 캐릭터 위치사수 명령
    public void Btn_Hold()
    {
        controlChar.isHold = !controlChar.isHold;

        if (controlChar.isActiveAndEnabled && controlChar.isHold && controlChar.isGather)
        {
            controlChar.StopGather();
            controlChar.AnimationSet(Anim.Idle);
        }

        if (controlChar.isHold)
            HoldButton.image.sprite = holdImages[0];
        else
            HoldButton.image.sprite = holdImages[1];
    }

    //카메라 홀드 버튼, CameraHoldCoroutine 호출
    public void Btn_CameraHold()
    {
        cameraHold = !cameraHold;
        swipeManager.CameraHold = cameraHold;

        if (cameraHold)
            StartCoroutine(CameraHoldCoroutine());
    }

    //집결 버튼, 집결 상태 켜기, 이 후 Update 함수에서 집결 처리
    public void Btn_Gather()
    {
        Time.timeScale = 0.4f;
        isGather = true;

        for (int i = 0; i < 3; i++)
            joystickSprites[i].enabled = false;
    }

    //카메라 위치 지금 조종 중인 캐릭터에 고정
    IEnumerator CameraHoldCoroutine()
    {
        while (cameraHold)
        {
            swipeManager.CameraPos(controlChar.transform);
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    //지금 조종 중인 캐릭터가 죽었을 때 호출
    private void CharChangeMain()
    {
        switch(nowChar)
        {
            case 0: //지금 조종 중이던 캐릭터가 0번 캐릭터일 때
                if (chars[1].isActiveAndEnabled)      //1번 캐릭터가 살아 있으면 1번 캐릭터로 변경
                    ChangeChar1();
                else if (chars[2].isActiveAndEnabled) //2번 캐릭터가 살아 있으면 2번 캐릭터로 변경
                    ChangeChar2();
                else
                    GameManager.instance.GameOver();  //다 죽었으면 GameOver
                break;
            case 1: //지금 조종 중이던 캐릭터가 1번 캐릭터일 때
                if (chars[0].isActiveAndEnabled)      //0번 캐릭터가 살아 있으면 0번 캐릭터로 변경
                    ChangeChar1();
                else if (chars[2].isActiveAndEnabled) //2번 캐릭터가 살아 있으면 2번 캐릭터로 변경
                    ChangeChar2();
                else
                    GameManager.instance.GameOver();  //다 죽었으면 GameOver
                break;
            case 2: //지금 조종 중이던 캐릭터가 2번 캐릭터일 때
                if (chars[0].isActiveAndEnabled)      //0번 캐릭터가 살아 있으면 0번 캐릭터로 변경
                    ChangeChar1();
                else if (chars[1].isActiveAndEnabled) //1번 캐릭터가 살아 있으면 1번 캐릭터로 변경
                    ChangeChar2();
                else
                    GameManager.instance.GameOver();  //다 죽었으면 GameOver
                break;
        }
    }

    //지금 조종 중인 캐릭터를 제외하고 앞에 있는 캐릭터로 변경   ex) 1번 캐릭터 조종 중이면 0번 2번 중 앞에 있는 0번으로 변경
    public void ChangeChar1()
    {
        if(!isChanging)
        {
            isChanging = true;
            
            Character tmp = chars[0];
            Btn_BasicOut();   //조종 중인 캐릭터의 공격 중지
            switch (nowChar)    //지금 조종 중인 캐릭터에 따라 판단
            {
                case 0: //지금 조종 중인 캐릭터가 0번 -> 1번 캐릭터로 변경
                    if (!chars[1].isActiveAndEnabled)   //1번 캐릭터가 죽어 있으면 함수 끝
                    {
                        isChanging = false;
                        return;
                    }

                    controlChar = chars[1];          //조종 캐릭터 1번으로 변경
                    chars[0].isControl = false;         //0번 캐릭터 조종 끄기
                    chars[0].isAttack = false;          //0번 캐릭터 공격 껐으니까 공격 중 bool 거짓으로 바꿈
                    chars[0].moveVector = Vector2.zero;
                    nowChar = 1;
                    break;
                case 1: //지금 조종 중인 캐릭터가 1번 -> 0번 캐릭터로 변경
                    tmp = chars[1];
                    if (!chars[0].isActiveAndEnabled)   //0번 캐릭터가 죽어 있으면 함수 끝
                    {
                        isChanging = false;
                        return;
                    }

                    controlChar = chars[0];          //조종 캐릭터 0번으로 변경
                    chars[1].isControl = false;         //1번 캐릭터 조종 끄기
                    chars[1].isAttack = false;          //1번 캐릭터 공격 껐으니까 공격 중 bool 거짓으로 바꿈
                    chars[1].moveVector = Vector2.zero;
                    nowChar = 0;
                    break;
                case 2: //지금 조종 중인 캐릭터가 2번 -> 0번 캐릭터로 변경
                    tmp = chars[2];
                    if (!chars[0].isActiveAndEnabled)   //0번 캐릭터가 죽어 있으면 함수 끝
                    {
                        isChanging = false;
                        return;
                    }

                    controlChar = chars[0];          //조종 캐릭터 0번으로 변경
                    chars[2].isControl = false;
                    chars[2].isAttack = false;
                    chars[2].moveVector = Vector2.zero;
                    nowChar = 0;
                    break;
            }

            SetButtonColor();

            controlChar.isControl = true;

            //추적 중이였으면 추적 끄기
            if (controlChar.isChase)
            {
                StopCoroutine(controlChar.chaseCoroutine);
                controlChar.isChase = false;
            }
            controlChar.isAttack = false;

            tmp.AttackMain();

            isChanging = false;
        }
    }

    //지금 조종 중인 캐릭터를 제외하고 뒤에 있는 캐릭터로 변경   ex) 1번 캐릭터 조종 중이면 0번 2번 중 뒤에 있는 2번으로 변경
    public void ChangeChar2()
    {
        if(!isChanging)
        {
            isChanging = true;

            Btn_BasicOut();   //조종 중인 캐릭터의 공격 중지
            Character tmp = chars[0];

            switch (nowChar)    //지금 조종 중인 캐릭터에 따라 판단
            {
                case 0: //지금 조종 중인 캐릭터가 0번 -> 2번 캐릭터로 변경
                    if (!chars[2].isActiveAndEnabled)   //2번 캐릭터가 죽어 있으면 함수 끝
                    {
                        isChanging = false;
                        return;
                    }

                    controlChar = chars[2];          //조종 캐릭터 2번으로 변경
                    chars[0].isControl = false;         //0번 캐릭터 조종 끄기
                    chars[0].isAttack = false;          //0번 캐릭터 공격 껐으니까 공격 중 bool 거짓으로 바꿈
                    chars[0].moveVector = Vector2.zero;
                    nowChar = 2;
                    break;
                case 1: //지금 조종 중인 캐릭터가 1번 -> 2번 캐릭터로 변경
                    tmp = chars[1];
                    if (!chars[2].isActiveAndEnabled)   //2번 캐릭터가 죽어 있으면 함수 끝
                    {
                        isChanging = false;
                        return;
                    }

                    controlChar = chars[2];          //조종 캐릭터 2번으로 변경
                    chars[1].isControl = false;         //1번 캐릭터 조종 끄기
                    chars[1].isAttack = false;          //1번 캐릭터 공격 껐으니까 공격 중 bool 거짓으로 바꿈
                    chars[1].moveVector = Vector2.zero;
                    nowChar = 2;
                    break;
                case 2: //지금 조종 중인 캐릭터가 2번 -> 1번 캐릭터로 변경
                    tmp = chars[2];
                    if (!chars[1].isActiveAndEnabled)   //1번 캐릭터가 죽어 있으면 함수 끝
                    {
                        isChanging = false;
                        return;
                    }

                    controlChar = chars[1];          //조종 캐릭터 1번으로 변경
                    chars[2].isControl = false;         //2번 캐릭터 조종 끄기
                    chars[2].isAttack = false;          //2번 캐릭터 공격 껐으니까 공격 중 bool 거짓으로 바꿈
                    chars[2].moveVector = Vector2.zero;
                    nowChar = 1;
                    break;
            }

            SetButtonColor();

            controlChar.isControl = true;

            //추적 중이였으면 추적 끄기
            if (controlChar.isChase)
            {
                StopCoroutine(controlChar.chaseCoroutine);
                controlChar.isChase = false;
            }
            controlChar.isAttack = false;

            tmp.AttackMain();

            isChanging = false;
        }
    }

    //CharacterChange 버튼의 색을 맞춰주는 함수, ChangeChar1, 2에서 호출
    private void SetButtonColor()
    {
        //홀드 버튼 이미지 변경
        if (controlChar.isHold)
            HoldButton.image.sprite = holdImages[0];
        else
            HoldButton.image.sprite = holdImages[1];

        //캐릭터 전환 시 카메라를 캐릭터로 이동
        swipeManager.CameraPos(controlChar.transform);

        ColorBlock cb1;
        ColorBlock cb2;
       
        Color char1Color = Color.white;  //지금 조종 중인 캐릭터 제외하고 앞에 있는 캐릭터의 색
        Color char2Color = Color.white;  //지금 조종 중인 캐릭터 제외하고 뒤에 있는 캐릭터의 색

        switch (nowChar)
        {
            case 0:
                char1Color = GetCharColor(1);
                char2Color = GetCharColor(2);
                break;
            case 1:
                char1Color = GetCharColor(0);
                char2Color = GetCharColor(2);
                break;
            case 2:
                char1Color = GetCharColor(0);
                char2Color = GetCharColor(1);
                break;
        }

        //지금 조종 중인 캐릭터 제외 앞에 있는 캐릭터의 색 반영
        cb1 = CCButton1.colors;
        cb1.normalColor = cb1.highlightedColor = cb1.pressedColor = char1Color;
        CCButton1.colors = cb1;

        //지금 조종 중인 캐릭터 제외 뒤에 있는 캐릭터의 색 반영
        cb2 = CCButton2.colors;
        cb2.normalColor = cb2.highlightedColor = cb2.pressedColor = char2Color;
        CCButton2.colors = cb2;

        ButtonImageSet();
    }

    //idx에 해당하는 캐릭터의 색 반환
    private Color GetCharColor(int idx)
    {
        Color tmpColor = Color.white;   //기본 흰색(무색)

        switch(chars[idx].charColor)
        {
            case ENUM.Color.Red: //빨강
                tmpColor = Color.red;
                break;
            case ENUM.Color.Orange: //주황
                tmpColor = new Color(255, 127, 0);
                break;
            case ENUM.Color.Yellow: //노랑
                tmpColor = Color.yellow;
                break;
            case ENUM.Color.Green: //녹색
                tmpColor = Color.green;
                break;
            case ENUM.Color.Blue: //파랑, 파랑 대신 일단 cyan 사용
                tmpColor = Color.cyan;
                break;
            case ENUM.Color.Navy: //남색, 남색 대신 일단 파랑 사용
                tmpColor = Color.blue;
                break;
            case ENUM.Color.Purple:
                tmpColor = new Color(127, 0, 200);
                break;
        }

        return tmpColor;
    }

    //현재 조종하는 캐릭터에 해당하는 버튼 이미지로 변경, SetButtonColor 함수에서 호출
    private void ButtonImageSet()
    {
        //기본 공격, 스킬 1, 2 버튼 이미지 적용
        switch (chars[nowChar].charClass)
        {
            case CharClass.Hero:
                BasicAttackButton.image.sprite = basicAttackImages[0];
                Skill1Button.image.sprite = skill1Images[0];
                Skill2Button.image.sprite = skill2Images[0];
                break;
            case CharClass.Chaser:
                BasicAttackButton.image.sprite = basicAttackImages[1];
                Skill1Button.image.sprite = skill1Images[1];
                Skill2Button.image.sprite = skill2Images[1];
                break;
            case CharClass.Crusher:
                BasicAttackButton.image.sprite = basicAttackImages[2];
                Skill1Button.image.sprite = skill1Images[2];
                Skill2Button.image.sprite = skill2Images[2];
                break;
        }

        //Character Change 버튼 적용
        switch (nowChar)
        {
            case 0:
                CCButton1.image.sprite = GetCCButtonSprite(1);
                CCButton2.image.sprite = GetCCButtonSprite(2);
                break;
            case 1:
                CCButton1.image.sprite = GetCCButtonSprite(0);
                CCButton2.image.sprite = GetCCButtonSprite(2);
                break;
            case 2:
                CCButton1.image.sprite = GetCCButtonSprite(0);
                CCButton2.image.sprite = GetCCButtonSprite(1);
                break;
        }


        BtnColorSet(BasicAttackButton);
        BtnColorSet(Skill1Button);
        BtnColorSet(Skill2Button);
    }

    //idx에 해당하는 캐릭터의 Change버튼 스프라이트 반환
    private Sprite GetCCButtonSprite(int idx)
    {
        Sprite tmpSprite = charChangeImages[0];
        switch (chars[idx].charClass)
        {
            case CharClass.Dummy:
                tmpSprite = charChangeImages[0];
                break;
            case CharClass.Hero:
                tmpSprite = charChangeImages[1];
                break;
            case CharClass.Chaser:
                tmpSprite = charChangeImages[2];
                break;
            case CharClass.Crusher:
                tmpSprite = charChangeImages[3];
                break;
        }

        return tmpSprite;
    }

    //스킬의 버튼 색을 반영하는 함수, ButtonImageSet에서 각각 호출
    private void BtnColorSet(Button btn)
    {
        Color tmpColor = GetCharColor(nowChar);

        ColorBlock cb = btn.colors;
        cb.highlightedColor = cb.normalColor = cb.pressedColor = tmpColor;

        btn.colors = cb;
    }

    private void SkillBtnCooldownSet()
    {
        ColorBlock cb0 = BasicAttackButton.colors;
        ColorBlock cb1 = Skill1Button.colors;
        ColorBlock cb2 = Skill2Button.colors;

        if(chars[nowChar].charClass == CharClass.Chaser)
        {
            if (chars[nowChar].GetComponent<Chaser>().nowWeapon)
            {
                BasicAttackButton.image.sprite = chaserClawAttackImage;
                Skill2Button.image.sprite = chaserClawSkill2Image;
            }
            else
            {
                BasicAttackButton.image.sprite = basicAttackImages[1];
                Skill2Button.image.sprite = skill2Images[1];
            }
        }

        /*
        for (int i = 0; i < 2; i++)
        {
            if (controlChar.cooldowns[i] == 0)
                SkillEdge[i].enabled = true;
            else
                SkillEdge[i].enabled = false;
        }
        */

        float basicImageAlpha = (1 - controlChar.basicCooldown * controlChar.totalStats[(int)CharStat.AttackSpeed]);
        float skill1ImageAlpha = (1 - controlChar.cooldowns[0] / controlChar.cooldownStart[0]);
        float skill2ImageAlpha = (1 - controlChar.cooldowns[1] / controlChar.cooldownStart[1]);

        basicImageAlpha = Mathf.Max(Mathf.Min(1, basicImageAlpha), 0);

        cb0.highlightedColor = cb0.normalColor = new Color(cb0.normalColor.r, cb0.normalColor.g, cb0.normalColor.b, basicImageAlpha);
        cb1.highlightedColor = cb1.normalColor = new Color(cb1.normalColor.r, cb1.normalColor.g, cb1.normalColor.b, skill1ImageAlpha);
        cb2.highlightedColor = cb2.normalColor = new Color(cb2.normalColor.r, cb2.normalColor.g, cb2.normalColor.b, skill2ImageAlpha);

        BasicAttackButton.colors = cb0;
        Skill1Button.colors = cb1;
        Skill2Button.colors = cb2;

        BasicAttackButton.image.fillAmount = basicImageAlpha;
        Skill1Button.image.fillAmount = skill1ImageAlpha;
        Skill2Button.image.fillAmount = skill2ImageAlpha;
    }
}