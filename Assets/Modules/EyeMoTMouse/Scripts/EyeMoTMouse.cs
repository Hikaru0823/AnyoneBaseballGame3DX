using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace EyeMoTMouseModule
{
    public class EyeMoTMouse : MonoBehaviour
    {
        public static EyeMoTMouse Instance { get; private set; }
        [Header("Assets")]
        [SerializeField] private Sprite[] icons = default;
        [SerializeField] private Image targetImage = default;
        [SerializeField] private Button gazeButton = default;

        private Image gazeButtonImage = default;
        private Keyboard keyboard = default;
        private Process cmdProcess = default;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        private bool isTrackable = false;
        private bool isInitialized = false;

        void Start()
        {
            Process[] eyeMoTProceses = Process.GetProcessesByName("EyeMoTMouse");

            if (eyeMoTProceses.Length > 0)
            {
                foreach (Process eyeMoTProcess in eyeMoTProceses)

                    //�v���Z�X�������I�ɏI��������
                    eyeMoTProcess.Kill();
            }

            if (Instance.cmdProcess == null)
            {
                Instance.cmdProcess = new Process();

                #if UNITY_WEBGL
                    Destroy(this.gameObject);
                    return;
                    //data.isEyeMotActive = false;
                #else
                    Instance.cmdProcess.StartInfo.FileName = Application.dataPath + "/../EyeMoTMouse/EyeMoTMouse.exe";

                    Instance.cmdProcess.StartInfo.Arguments = "30";

                    // this.cmdProcess.StartInfo.CreateNoWindow = true; 

                    Instance.cmdProcess.EnableRaisingEvents = true;

                    Instance.cmdProcess.Exited += CmdProcessExited;

                    Instance.cmdProcess.StartInfo.UseShellExecute = false;

                    Instance.cmdProcess.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;

                    Instance.cmdProcess.StartInfo.RedirectStandardOutput = true;

                    Instance.cmdProcess.StartInfo.RedirectStandardInput = true;

                    // �W���o�̓C�x���g�ݒ�.
                    Instance.cmdProcess.OutputDataReceived += OutputHandler;

                    Instance.cmdProcess.Start();

                    Instance.cmdProcess.BeginOutputReadLine();

                    gazeButtonImage = Instance.targetImage.GetComponent<Image>();
                    Instance.keyboard = Keyboard.current;

                    Instance.OnStatusChanged(!Instance.isTrackable);
                    Instance.isInitialized = true;
                    #endif
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Instance.keyboard != null)
            {
                if (Instance.keyboard.xKey.wasReleasedThisFrame)
                    Instance.OnStatusChanged(Instance.isTrackable);
            }
        }

        // EyeMoTMouse�̃R�}���h���C���ɉ����������܂ꂽ�Ƃ��ɓ��삷��
        private void OutputHandler(object sender, DataReceivedEventArgs args)
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                string trimedArgs = args.Data.Trim(); // �擾���������񂩂�󔒕�������s������؂藎�Ƃ�����

                // 0: �I���@1: �I�t
                switch (trimedArgs)
                {
                    case "0":
                        return;

                    case "1":
                        return;

                    case "StartUp":
                        // this.cmdProcess.StandardInput.WriteLine("mouse_off");
                        return;
                }
            }
        }

        void CmdProcessExited(object sender, System.EventArgs e)
        {
            Instance.cmdProcess.Dispose();
            Instance.cmdProcess = null;
        }

        private void OnApplicationQuit()
        {
            #if UNITY_WEBGL
        #else
            if (Instance.cmdProcess != null)
            {
                Instance.cmdProcess.StandardInput.WriteLine("exit");

                //�v���Z�X���I������܂ōő�15�b�ҋ@����
                Instance.cmdProcess.WaitForExit(1500);
                Instance.cmdProcess.Kill();
                Instance.cmdProcess.Dispose();
                Instance.cmdProcess = null;
            }
        #endif
        }

        public void OnButtonClicked(Button button)
        {
            switch (button.name)
            {
                case "GazeButton":

                    Instance.OnStatusChanged(Instance.isTrackable);

                    break;
            }
        }

        private void OnStatusChanged(bool isTrackable)
        {
            if (isTrackable)
            {
                Instance.gazeButtonImage.sprite = Instance.icons[0];
                Instance.cmdProcess.StandardInput.WriteLine("mouse_off");
                Instance.isTrackable = false;
            }
            else
            {
                Instance.gazeButtonImage.sprite = Instance.icons[1];
                Instance.cmdProcess.StandardInput.WriteLine("mouse_on");
                Instance.isTrackable = true;
            }
        }
    }
}
