using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemToken
{
    public int idx;     //save manager에서의 index
}

public class SmithManager : MonoBehaviour
{
    public GameObject buttonPrefab;

    int count;                      //현재 버튼의 갯수, 아이템 제작, 분해 시 변함
    public GameObject content;      //scroll view content 항목, button들의 parent
    public Button[] buttons = new Button[200];

    List<Button> buttonPool = new List<Button>();

    List<ItemToken> itemSort = new List<ItemToken>();

    private void Start()
    {
        ButtonInstantiate();
    }

    //처음 버튼 생성
    void ButtonInstantiate()
    {
        for (count = 0; count < SaveManager.instance.itemsave.count; count++)
        {
            buttons[count] = Instantiate(buttonPrefab).GetComponent<Button>();

            buttons[count].transform.parent = content.transform;
            buttons[count].transform.localPosition = new Vector3(105 + (count % 4) * 145, -100 - (count / 4) * 150, 0);
            buttons[count].transform.localScale = new Vector3(1, 1, 1);
        }
    }

    //장비 제작 또는 구매 -> 버튼 하나 추가
    public void ButtonAdd()
    {
        if (buttonPool.Count > 0)
        {
            buttons[count] = buttonPool[0];
            buttonPool.RemoveAt(0);
        }
        else
            buttons[count] = Instantiate(buttonPrefab).GetComponent<Button>();

        buttons[count].transform.parent = content.transform;
        buttons[count].transform.localPosition = new Vector3(105 + (count % 4) * 145, -100 - (count / 4) * 150, 0);
        buttons[count].transform.localScale = new Vector3(1, 1, 1);
        buttons[count++].gameObject.SetActive(true);
    }

    //장비 분해 -> 버튼 하나 제거
    public void ButtonRemove()
    {
        count--;
        buttons[count].gameObject.SetActive(false);
        buttonPool.Add(buttons[count]);
    }
}
