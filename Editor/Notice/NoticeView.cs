using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace UnityExtended
{
    public class NoticeView
    {
        private EditorWindow window;
        private GUIContent noticeContent;
		private Rect position;
        private long lastTime;
        private bool showNotice;
        private float fillAmount;
        private int taskID;

        public NoticeView(EditorWindow window) => this.window = window ?? throw new System.ArgumentNullException(nameof(window));

        public async void OnGUI()
		{
			if(!showNotice) { return; }

			long now = System.DateTime.Now.Ticks;
			float deltaTime = (now - lastTime) / (float)System.TimeSpan.TicksPerSecond;
			lastTime = now;

			if (fillAmount != 1f)
			{
				fillAmount = Mathf.MoveTowards(fillAmount, 1f, deltaTime * 2);
			}

			position.y = position.height * (fillAmount - 0.8f);

			GUILayout.BeginArea(position);
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label(noticeContent, "NotificationBackground");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.EndArea();

			if (fillAmount == 1f)
			{
				var waitID = taskID;
				await Task.Delay(1000);
				if (waitID == taskID)
				{
					fillAmount = 0f;
					showNotice = false;
				}
			}

			window.Repaint();
		}

		public void Show(Rect position, GUIContent content)
		{
			this.position = position;
			noticeContent = content;
			taskID++;
			showNotice = true;
			fillAmount = 0f;
			lastTime = System.DateTime.Now.Ticks;
		}
	}
}