using UnityEngine;
using TMPro;

public class CenterMessageView : MonoBehaviour
{

    [SerializeField]
    private GameObject rootObject;

    [SerializeField]
    private TMP_Text messageText;

    public void ShowMessage(string message)
    {

        if (messageText != null)
        {

            messageText.text = message;

        }

        if (rootObject != null)
        {

            rootObject.SetActive(true);

        }
        else
        {

            gameObject.SetActive(true);

        }

    }

    public void HideMessage()
    {

        if (rootObject != null)
        {

            rootObject.SetActive(false);

        }
        else
        {

            gameObject.SetActive(false);

        }

    }

}

