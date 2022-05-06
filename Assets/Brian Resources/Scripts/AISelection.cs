using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AISelection : MonoBehaviour
{
    public string side;
    public string selectedDLL;

    public bool flipped = false;

    [SerializeField] private RectTransform itemHolderPlayer;

    [SerializeField] private GameObject itemPrefabRight;

    [SerializeField] private Image selectionImage;

    public readonly float maxHeight = 1300f;
    public readonly float itemHeight = 220f;

    public RectTransform[] itemsPlayer; 

    public List<string> dlls;

    public int selectedItem;
    public int selectedIndex { get { return count - selectedItem - 1; } } 

    // Update is called once per frame
    void Update()
    {
        UpdateItemPositions(); 
        UpdateBoxSelect();
    }

    private RectTransform CreateItem(GameObject prefab, Transform holder, int position, string name)
    { 
        var item = Instantiate(prefab, holder);
        item.name = side + ":" + position.ToString() + ":" + name;
        var transform = item.GetComponent<RectTransform>();
        var y = -(contentHeight/2) + (maxHeight / 2) + (position * itemHeight) + (itemHeight/2);
        transform.anchoredPosition = new Vector2(transform.anchoredPosition.x, y);

        return transform;
    }

    private float contentHeight;
    private int count;

    

    public void Initialize(List<string> dlls)
    {
        this.dlls = dlls;

        count = dlls.Count;
        itemsPlayer = new RectTransform[count];
        contentHeight = itemHeight * count + maxHeight; 
        itemHolderPlayer.sizeDelta = new Vector2(itemHolderPlayer.sizeDelta.x, contentHeight); 

        //item creation 
        for (int i = 0; i < count; i++) 
        {
            //Since the creation is bottom to top, I must invert the index
            var dll = dlls[count - i - 1];
            var key = BritoUtil.GetName(dll);
            // FOR 1
            var item = CreateItem(itemPrefabRight, itemHolderPlayer, i, key);
            itemsPlayer[i] = item;
            item.GetComponentInChildren<TMPro.TMP_Text>().text = key; 
        }  

        //START
        SetContent(itemHolderPlayer, 0);
        SetContent(0);
    }

    private void UpdateItemPositions()
    { 
        for (int i = 0; i < count; i++)
        {
            var yPos = itemHolderPlayer.anchoredPosition.y;

            var item1 = itemsPlayer[i];
            var y = yPos - (contentHeight/2 - item1.anchoredPosition.y - maxHeight/2); 
            var x = !flipped ? .0009f * Mathf.Pow(y, 2) : -.0005f * Mathf.Pow(y, 2); 

            item1.anchoredPosition = new Vector2(x, item1.anchoredPosition.y);   
        } 

    }

    private void SetContent(RectTransform holder, int index)
    {
        float height = index * itemHeight + itemHeight/2;  
        holder.anchoredPosition = new Vector2(holder.anchoredPosition.x, height); 
    }

    public void SetContent(int index)
    { 
        selectedItem = index;
        selectedDLL = dlls[index];
    }


    private Vector2 startPos;
    private void UpdateBoxSelect()
    {

        if (Input.GetMouseButtonDown(0))
            startPos = Input.mousePosition;

        if (Input.GetMouseButtonUp(0))
        { 
            var endPos = Input.mousePosition;
            var dist = Vector2.Distance(startPos, endPos);
            if (dist <= .1f)
                return; 

            var yPos = itemHolderPlayer.anchoredPosition.y;
            float y = yPos;

            int index = (int)(y / itemHeight);
            SetContent(index);
        }

        float selectionAlpha = .4f;
        var prevY = itemHolderPlayer.anchoredPosition.y;
        //This is updating the position 
        if (!Input.GetMouseButton(0))
        {
            float y = selectedItem * itemHeight + itemHeight / 2;
            float diff = itemHolderPlayer.anchoredPosition.y - y;
            float absDiff = Mathf.Abs(diff);
            if (absDiff >= 2)
            {
                var dir = diff > 0 ? -1 : 1;
                itemHolderPlayer.anchoredPosition = new Vector2(itemHolderPlayer.anchoredPosition.x, 600f * dir * Time.deltaTime + itemHolderPlayer.anchoredPosition.y);
            }
            else if (absDiff < 2)
                selectionAlpha = 1.0f;
        }
        selectionImage.color = new Color(selectionImage.color.r, selectionImage.color.g, selectionImage.color.b, selectionAlpha);
    }
}
