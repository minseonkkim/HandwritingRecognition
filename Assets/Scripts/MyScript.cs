using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;


public class MyScript : MonoBehaviour
{
    Texture2D my_texture;
    Texture2D my_texture2;

    Rect my_input_rect;
    Rect my_normalize_rect;

    //바깥쪽에서 사용자가 입력한 입력값
    public UnityEngine.UI.InputField model_name;

    //바깥쪽 UI에 표시할 인식 결과
    public UnityEngine.UI.InputField check_result;


    //숫자 모델 저장
    List<float[]> digit_models = new List<float[]>();


    void Start()
    {
        my_texture = MakeTex(32, 32, Color.white);
        my_texture2 = MakeTex(8, 8, Color.black);

        my_input_rect = new Rect();
        my_input_rect.x = Screen.width * 0.4f;
        my_input_rect.y = Screen.height * 0.2f;
        my_input_rect.width = 256;
        my_input_rect.height = 256;

        my_normalize_rect = new Rect();
        my_normalize_rect.x = Screen.width * 0.55f;
        my_normalize_rect.y = Screen.height * 0.2f;
        my_normalize_rect.width = 256;
        my_normalize_rect.height = 256;
    }


    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }


    private void CheckMouseClick()
    {
        if (Input.GetMouseButton(0))
        {
            Vector2 mouse_pos = new Vector2();
            mouse_pos.x = Input.mousePosition.x;
            mouse_pos.y = Screen.height - Input.mousePosition.y;

            if (my_input_rect.Contains(mouse_pos))
            {
                float x = mouse_pos.x - my_input_rect.x;
                float y = 256 - (mouse_pos.y - my_input_rect.y);

                //32x32
                x = x / 8;
                y = y / 8;

                //8x8
                float x2 = x / 4;
                float y2 = y / 4;

                //32x32
                int pos = (int)y * 32 + (int)x;

                //8x8
                int pos2 = (int)y2 * 8 + (int)x2;

                //32x32
                Color[] pixels = my_texture.GetPixels();
                pixels[pos] = Color.black;

                //8x8
                Color[] pixels2 = my_texture2.GetPixels();
                pixels2[pos2] = Color.white;

                my_texture.SetPixels(pixels);
                my_texture.Apply();

                my_texture2.SetPixels(pixels2);
                my_texture2.Apply();
            }
        }
    }


    void Update()
    {
        CheckMouseClick();
    }

    private void OnGUI()
    {
        GUI.DrawTexture(my_input_rect, my_texture);
        GUI.DrawTexture(my_normalize_rect, my_texture2);
    }


    public void ClearTexture()
    {
        my_texture = MakeTex(32, 32, Color.white);
        my_texture2 = MakeTex(8, 8, Color.black);
    }


    public void AddModel()
    {
        float[] model_val = new float[65];

        Color[] pixels2 = my_texture2.GetPixels();

        for (int i = 0; i < pixels2.Length; i++)
        {
            model_val[i] = pixels2[i].r + pixels2[i].g + pixels2[i].b;
        }

        model_val[64] = int.Parse(model_name.text);

        digit_models.Add(model_val);

        Debug.Log(model_name.text + " added!");
    }


    public void Test()
    {
        float[] model_val = new float[64];
        Color[] pixels2 = my_texture2.GetPixels();

        for (int i = 0; i < pixels2.Length; i++)
        {
            model_val[i] = pixels2[i].r + pixels2[i].g + pixels2[i].b;
        }


        float min_ind = -1;
        float min_value = float.MaxValue;

        for (int m = 0; m < digit_models.Count; m++)
        {
            float[] targets = digit_models[m];

            float sum = 0;

            for (int a = 0; a < 64; a++)
                sum = sum + Mathf.Abs(targets[a] - model_val[a]);

            if (sum < min_value)
            {
                min_value = sum;
                min_ind = targets[64];
            }
        }

        if (min_ind >= 0)
        {
            Debug.Log(min_ind.ToString() + " checked!");
            check_result.text = min_ind.ToString();
        }
        else
            Debug.Log("no checked!");

    }


    public void SaveModel()
    {
        string model_res = string.Empty;

        for (int i = 0; i < digit_models.Count; i++)
        {
            string line = string.Empty;

            float[] val = digit_models[i];

            for (int a = 0; a < 65; a++)
            {
                line = line + val[a] + ",";
            }

            model_res = model_res + line + System.Environment.NewLine;
        }


        string model_file_name = "C:/Models/digit_model.txt";

        if (Directory.Exists("C:/Models") == false)
        {
            Directory.CreateDirectory("C:/Models");
        }

        if (File.Exists(model_file_name))
        {
            File.Delete(model_file_name);
        }

        File.WriteAllText(model_file_name, model_res);

        Debug.Log("model saved!");
    }


    public void LoadModel()
    {
        if (File.Exists("C:/Models/digit_model.txt") == false)
            return;

        string read_txt = File.ReadAllText("C:/Models/digit_model.txt");

        string[] lines = read_txt.Split(new char[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);


        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            string[] vals = line.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);

            if (vals != null && vals.Length == 65)
            {
                float[] model_val = new float[65];

                for (int a = 0; a < 65; a++)
                    model_val[a] = float.Parse(vals[a]);

                digit_models.Add(model_val);
            }
        }

        Debug.Log("model loaded!");
    }


}
