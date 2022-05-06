using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AISelection : MonoBehaviour
{
    public bool flipped = false;

    [SerializeField] private RectTransform itemHolderPlayer;

    [SerializeField] private GameObject itemPrefabRight;

    [SerializeField] private Image selectionImage;

    public readonly float maxHeight = 1300f;
    public readonly float itemHeight = 220f;

    [SerializeField] private RectTransform[] itemsPlayer;

    public int selectedItem;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        UpdateItemPositions();

        UpdateBoxSelect();
    }

    private RectTransform CreateItem(GameObject prefab, Transform holder, int position, string name)
    {
        var item = Instantiate(prefab, holder);
        item.name = position.ToString();
        var transform = item.GetComponent<RectTransform>();
        var y = -(contentHeight/2) + (maxHeight / 2) + (position * itemHeight) + (itemHeight/2);
        transform.anchoredPosition = new Vector2(transform.anchoredPosition.x, y);

        return transform;
    }

    private float contentHeight;
    private int count;

    public void Initialize(SortedDictionary<string, string> dlls)
    {

        count = dlls.Count;
        itemsPlayer = new RectTransform[count];
        contentHeight = itemHeight * count + maxHeight;
        //Set size
        itemHolderPlayer.sizeDelta = new Vector2(itemHolderPlayer.sizeDelta.x, contentHeight);

        int j = 0;
        foreach (var dll in dlls)
        {
            // FOR 1
            var item = CreateItem(itemPrefabRight, itemHolderPlayer, j, dll.Key);
            itemsPlayer[j] = item;
            item.GetComponentInChildren<TMPro.TMP_Text>().text = dll.Key;

            j++;
        } 

        //START
        SetContent(itemHolderPlayer, 0);
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

    private void SetContent(int index)
    {
        //float height = index * itemHeight + itemHeight / 2;
        selectedItem = index;        
        //holder.anchoredPosition = new Vector2(holder.anchoredPosition.x, height);
    }


    private void UpdateBoxSelect()
    {

        if (Input.GetMouseButtonUp(0))
        {
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
            else if(absDiff < 2)
            {
                SetContent(selectedItem); selectionAlpha = 1.0f;
            }
        }
        else
        {

        }

        //if (itemHolderPlayer.anchoredPosition.y == prevY) selectionAlpha = 1.0f;
        //else selectionAlpha = .4f;

        selectionImage.color = new Color(selectionImage.color.r, selectionImage.color.g, selectionImage.color.b, selectionAlpha);
    }
}
