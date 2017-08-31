using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;
using System.Text;

public class TMP_test : MonoBehaviour, IPointerClickHandler
{


    private bool m_isHoveringObject;
    private TMP_Text m_TextMeshPro ;
    private Vector3 pos = new Vector3(634, 360, 0);
    private bool colored = false;
    private string focused=string.Empty;
    void Start()
    {
        m_TextMeshPro = this.gameObject.GetComponent<TMP_InputField>().textComponent;
    }
    void Update()
    {
        m_isHoveringObject = false;

        if (TMP_TextUtilities.FindIntersectingLink(m_TextMeshPro, pos, Camera.main)!=-1)
        {
            Debug.Log("ishover");
            m_isHoveringObject = true;
        }
        else {
            
            string description = this.gameObject.GetComponent<TMP_InputField>().text;
            //Debug.Log("substring: " + "<color=blue>" + focused + "</color>");
            //Debug.Log("description: " + "<color=blue>" + focused + "</color>");
            if (description.Contains("<color=blue>" + focused + "</color>"))
            {
                //Debug.Log("focused: "+focused);
                //Debug.Log("contains: "+ "<color=blue>" + focused + "</color>");

                StringBuilder builder = new StringBuilder(description);
                builder.Replace("<color=blue>" + focused + "</color>", focused);
                description = builder.ToString();
                this.gameObject.GetComponent<TMP_InputField>().text = description;
                //Debug.Log("description after: " + description);
                focused = string.Empty;
                colored = false;
            }
            
        }

        if (m_isHoveringObject && colored == false)
        {
            Debug.Log("update link color");
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(m_TextMeshPro, pos, Camera.main);

            if (linkIndex != -1)
            {

                TMP_LinkInfo linkInfo = this.gameObject.GetComponent<TMP_InputField>().textComponent.textInfo.linkInfo[linkIndex];
                string description = this.gameObject.GetComponent<TMP_InputField>().text;
                if (description.Contains(linkInfo.GetLinkText()))
                {
                    StringBuilder builder = new StringBuilder(description);
                    focused = linkInfo.GetLinkText();
                    builder.Replace(linkInfo.GetLinkText(), "<color=blue>" + linkInfo.GetLinkText() + "</color>");
                    description = builder.ToString();
                    this.gameObject.GetComponent<TMP_InputField>().text = description;
                    colored = true;
                }

            }


        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(m_TextMeshPro, pos, Camera.main);
        UnityEngine.Debug.Log(linkIndex);
        if (linkIndex != -1) {

            TMP_LinkInfo linkInfo = this.gameObject.GetComponent<TMP_InputField>().textComponent.textInfo.linkInfo[linkIndex];
            UnityEngine.Debug.Log("linkinfo: "+linkInfo.GetLinkText());
#if WINDOWS_UWP
Windows.System.Launcher.LaunchUriAsync(new System.Uri(linkInfo.GetLinkText()));
#endif

        }
    }
}