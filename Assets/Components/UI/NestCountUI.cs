using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Antymology.Terrain;

namespace Antymology.UI
{
    public class NestCountUI : MonoBehaviour
    {
        // Use ONE of these depending on what you added to the scene:
        public TextMeshProUGUI nestCountText;  // For TextMeshPro
        // public Text nestCountText;          // For legacy UI Text

        void Update()
        {
            if (nestCountText != null && WorldManager.Instance != null)
            {
                nestCountText.text = $"Nest Blocks: {WorldManager.Instance.nestBlockCount}";
            }
        }
    }
}
