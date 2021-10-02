using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Reflection;
using System.Collections;
using UnityEditor;
using System.IO;
using Newtonsoft.Json;

public class UIHandler : MonoBehaviour
{
    //5 total cameras 0,1,2,3,4
    private int total_cameras = 4;
    //the default selected camera is free camera (index = 0)
    private int camera_index = 0;
    private bool ui_hidden = false;

    //Cameras Names
    private string[] cameras = { "Free Camera", "Car1 Camera", "Car2 Camera", "Car3 Camera", "Car4 Camera" };

    //General UI Compoenents
    [SerializeField] private Slider time_scale_slider;
    [SerializeField] private TextMeshProUGUI time_scale_text;
    [SerializeField] private TextMeshProUGUI camera_name_text;
    [SerializeField] private Canvas UI_Canvas;

    //Free Look Camera reference
    [SerializeField] private Camera free_look_cam;

    //Car cameras references
    [SerializeField] private Camera car_one_cam;
    [SerializeField] private Camera car_two_cam;
    [SerializeField] private Camera car_three_cam;
    [SerializeField] private Camera car_four_cam;


    //Change camera buttons references
    [SerializeField] private Button toggle_left;
    [SerializeField] private Button toggle_right;

    //Exit , Restart ui buttons references 
    [SerializeField] private Button exit_button;
    [SerializeField] private Button restart_button;

    //UI Toggle reference for showing or hiding car sensor lines
    [SerializeField] private Toggle show_sensors_toggle;

    //Panel ui text references
    [SerializeField] private GameObject info_panel;
    [SerializeField] private TextMeshProUGUI panel_title_text;
    [SerializeField] private TextMeshProUGUI turn_value_text;
    [SerializeField] private TextMeshProUGUI acceleration_value_text;
    [SerializeField] private TextMeshProUGUI time_value_text;
    [SerializeField] private TextMeshProUGUI fitness_value_text;
    [SerializeField] private TextMeshProUGUI generation_value_text;
    [SerializeField] private TextMeshProUGUI genome_value_text;
    [SerializeField] private TextMeshProUGUI train_time_text;

    //Cars gameobject references
    [SerializeField] private GameObject car1;
    [SerializeField] private GameObject car2;
    [SerializeField] private GameObject car3;
    [SerializeField] private GameObject car4;

    //CarController script variables for each car script
    private CarController carController1, carController2, carController3, carController4;

    private bool saved_stats = false;

    // Start is called before the first frame update
    void Start()
    {
        //Hide the car information panel at the start of the program
        info_panel.SetActive(false);

        //Get CarController script reference for each of the cars
        carController1 = car1.GetComponent<CarController>();
        carController2 = car2.GetComponent<CarController>();
        carController3 = car3.GetComponent<CarController>();
        carController4 = car4.GetComponent<CarController>();

        //Add event listeners to the UI components (buttons,toggles)
        time_scale_slider.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
        toggle_left.onClick.AddListener(TaskOnToggleLeftClick);
        toggle_right.onClick.AddListener(TaskOnToggleRightClick);
        exit_button.onClick.AddListener(TaskOnExitClick);
        restart_button.onClick.AddListener(TaskOnRestartClick);
        show_sensors_toggle.onValueChanged.AddListener((value) => { 
            handleCheckbox(value); 
        }  
        );
    }

    //Save the car statistics in txt files with a json format in a folder called CarStats in the desktop of the computer
    public void SaveCarsStatistics()
    {
        string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        if (!System.IO.Directory.Exists(path + "\\CarStats"))
        {
            Directory.CreateDirectory(path + "\\CarStats");
        }
        path = path + "\\CarStats";
        string ID = DateTime.Now.Ticks.ToString();

        string[,] car1_stats = {{"Car 1", "TimeSinceStart: " + FormatTime(carController1.timeSinceStart) , "Fitness: " + carController1.overallFitness.ToString(), "Generation: " + carController1.genetic_manager.currentGeneration.ToString(),"Genome: " + carController1.genetic_manager.currentGenome.ToString(), "TrainTime: ",FormatTime(carController1.time_to_train)},
        { "Car 2", "TimeSinceStart: " + FormatTime(carController2.timeSinceStart) , "Fitness: " + carController2.overallFitness.ToString(), "Generation: " + carController2.genetic_manager.currentGeneration.ToString(),"Genome: " + carController2.genetic_manager.currentGenome.ToString(), "TrainTime: ",FormatTime(carController2.time_to_train)},
        { "Car 3", "TimeSinceStart: " + FormatTime(carController3.timeSinceStart) , "Fitness: " + carController3.overallFitness.ToString(), "Generation: " + carController3.genetic_manager.currentGeneration.ToString(),"Genome: " + carController3.genetic_manager.currentGenome.ToString(), "TrainTime: ",FormatTime(carController3.time_to_train)},
        { "Car 4", "TimeSinceStart: " + FormatTime(carController4.timeSinceStart) , "Fitness: " + carController4.overallFitness.ToString(), "Generation: " + carController4.genetic_manager.currentGeneration.ToString(),"Genome: " + carController4.genetic_manager.currentGenome.ToString(), "TrainTime: ",FormatTime(carController4.time_to_train)}
        };
        string json = JsonConvert.SerializeObject(car1_stats);
        File.WriteAllText(path + "\\cars_statistics_" + ID + ".txt", json);
    }

    //Get a time as a float value , convert it and return it as a string value of format 00:00:000 - minutes:seconds:fraction
    string FormatTime(float time)
    {
        int intTime = (int)time;
        int minutes = intTime / 60;
        int seconds = intTime % 60;
        float fraction = time * 1000;
        fraction = (fraction % 1000);
        string timeText = String.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, fraction);
        return timeText;
    }

    // Update is called once per frame
    void Update()
    {
        //If T key is pressed hide or show the main UI based on its current state
        if (Input.GetKeyDown(KeyCode.T)){
            if (ui_hidden == false)
            {
                UI_Canvas.enabled = ui_hidden;
                ui_hidden = true;
            }
            else
            {
                UI_Canvas.enabled = ui_hidden;
                ui_hidden = false;
            }
        }
        //Update the panel information texts based on the current car selected script variables
        //camera_index == 1 -> Show information of the Car 1 carController script variables
        //camera_index == 2 -> Show information of the Car 2 carController script variables
        //camera_index == 3 -> Show information of the Car 3 carController script variables
        //camera_index == 4 -> Show information of the Car 4 carController script variables
        if (camera_index == 1)
        {
            if (carController1 != null)
            {
                turn_value_text.text = carController1.turning.ToString();
                acceleration_value_text.text = carController1.acceleration.ToString();
                time_value_text.text = FormatTime(carController1.timeSinceStart);
                fitness_value_text.text = carController1.overallFitness.ToString();
                generation_value_text.text = carController1.genetic_manager.currentGeneration.ToString();
                genome_value_text.text = carController1.genetic_manager.currentGenome.ToString();
                train_time_text.text = FormatTime(carController1.time_to_train);
            }
        }
        else if (camera_index == 2)
        {
            if (carController2 != null)
            {
                turn_value_text.text = carController2.turning.ToString();
                acceleration_value_text.text = carController2.acceleration.ToString();
                time_value_text.text = FormatTime(carController2.timeSinceStart);
                fitness_value_text.text = carController2.overallFitness.ToString();
                generation_value_text.text = carController2.genetic_manager.currentGeneration.ToString();
                genome_value_text.text = carController2.genetic_manager.currentGenome.ToString();
                train_time_text.text = FormatTime(carController2.time_to_train);
            }
        }
        else if (camera_index == 3)
        {
            if (carController3 != null)
            {
                turn_value_text.text = carController3.turning.ToString();
                acceleration_value_text.text = carController3.acceleration.ToString();
                time_value_text.text = FormatTime(carController3.timeSinceStart);
                fitness_value_text.text = carController3.overallFitness.ToString();
                generation_value_text.text = carController3.genetic_manager.currentGeneration.ToString();
                genome_value_text.text = carController3.genetic_manager.currentGenome.ToString();
                train_time_text.text = FormatTime(carController3.time_to_train);
            }
        }
        else if (camera_index == 4)
        {
            if (carController4 != null)
            {
                turn_value_text.text = carController4.turning.ToString();
                acceleration_value_text.text = carController4.acceleration.ToString();
                time_value_text.text = FormatTime(carController4.timeSinceStart);
                fitness_value_text.text = carController4.overallFitness.ToString();
                generation_value_text.text = carController4.genetic_manager.currentGeneration.ToString();
                genome_value_text.text = carController4.genetic_manager.currentGenome.ToString();
                train_time_text.text = FormatTime(carController4.time_to_train);
            }
        }
        //When all cars reach the set train goal then save their stats to the desktop
        if(saved_stats == false)
        {
            if(carController1.saved == true && carController2.saved == true && carController3.saved == true && carController4.saved == true)
            {
                saved_stats = true;
                SaveCarsStatistics();
            }
        }
    }

    //For each car show or hide sensor lines by changing the
    //showSensors variable of CarController script for each car
    //based of the checkbox value
    public void handleCheckbox(bool val)
    {
        if (carController1 != null)
        {
            carController1.showSensors = val;
        }
        if (carController2 != null)
        {
            carController2.showSensors = val;
        }
        if (carController3 != null)
        {
            carController3.showSensors = val;
        }
        if (carController4 != null)
        {
            carController4.showSensors = val;
        }
    }

    // Invoked when the value of the slider changes.
    public void ValueChangeCheck()
    {
        //Change the timescale based of the slider current value
       Time.timeScale = time_scale_slider.value;
       time_scale_text.text = time_scale_slider.value.ToString();
    }

    //If right button is pressed go to the previous camera
    //If there is no previous camera (index=0) then go to the
    //last camera (index = 4)
    void TaskOnToggleLeftClick()
    {
        if(camera_index > 0)
        {
            camera_index -= 1;
        }
        else
        {
            camera_index = total_cameras;
        }
        ChangeCamera();
    }

    //If left button is pressed go to the next camera
    //If there is no next camera (index=4) then go back to the
    //start camera (index = 0)
    void TaskOnToggleRightClick()
    {
        if (camera_index < total_cameras)
        {
            camera_index += 1;
        }
        else
        {
            camera_index = 0;
        }
        ChangeCamera();
    }

    //If exit button is pressed then quit the application
    void TaskOnExitClick()
    {
        Application.Quit();
    }

    //If restart button is pressed then reload the level
    void TaskOnRestartClick()
    {
        Application.LoadLevel(0);
    }

    //This function is called every time the user changes camera with the ui buttons
    private void ChangeCamera()
    {
        //if free look camera is selected
        if(camera_index == 0)
        {
            //Enable free look camera and disable all the others
            free_look_cam.enabled = true;
            car_one_cam.enabled = false;
            car_two_cam.enabled = false;
            car_three_cam.enabled = false;
            car_four_cam.enabled = false;

            //Enable audio listener of the free look camera and disable all the others
            free_look_cam.GetComponent<AudioListener>().enabled = true;
            car_one_cam.GetComponent<AudioListener>().enabled = false;
            car_two_cam.GetComponent<AudioListener>().enabled = false;
            car_three_cam.GetComponent<AudioListener>().enabled = false;
            car_four_cam.GetComponent<AudioListener>().enabled = false;

            //On free look camera the information panel is hidden
            info_panel.SetActive(false);
        }
        else if(camera_index == 1)
        {
            //Enable car 1 camera and disable all the others
            free_look_cam.enabled = false;
            car_one_cam.enabled = true;
            car_two_cam.enabled = false;
            car_three_cam.enabled = false;
            car_four_cam.enabled = false;

            //Enable audio listener of the car 1 camera and disable all the others
            free_look_cam.GetComponent<AudioListener>().enabled = false;
            car_one_cam.GetComponent<AudioListener>().enabled = true;
            car_two_cam.GetComponent<AudioListener>().enabled = false;
            car_three_cam.GetComponent<AudioListener>().enabled = false;
            car_four_cam.GetComponent<AudioListener>().enabled = false;

            //On car 1 camera the information panel is visible
            info_panel.SetActive(true);
            //Change the panel title based on the car selected (Car 1)
            panel_title_text.text = "Information about Car 1";
        }
        else if (camera_index == 2)
        {
            //Enable car 2 camera and disable all the others
            free_look_cam.enabled = false;
            car_one_cam.enabled = false;
            car_two_cam.enabled = true;
            car_three_cam.enabled = false;
            car_four_cam.enabled = false;

            //Enable audio listener of the car 2 camera and disable all the others
            free_look_cam.GetComponent<AudioListener>().enabled = false;
            car_one_cam.GetComponent<AudioListener>().enabled = false;
            car_two_cam.GetComponent<AudioListener>().enabled = true;
            car_three_cam.GetComponent<AudioListener>().enabled = false;
            car_four_cam.GetComponent<AudioListener>().enabled = false;

            //On car 2 camera the information panel is visible
            info_panel.SetActive(true);
            //Change the panel title based on the car selected (Car 2)
            panel_title_text.text = "Information about Car 2";
        }
        else if (camera_index == 3)
        {
            //Enable car 3 camera and disable all the others
            free_look_cam.enabled = false;
            car_one_cam.enabled = false;
            car_two_cam.enabled = false;
            car_three_cam.enabled = true;
            car_four_cam.enabled = false;

            //Enable audio listener of the car 3 camera and disable all the others
            free_look_cam.GetComponent<AudioListener>().enabled = false;
            car_one_cam.GetComponent<AudioListener>().enabled = false;
            car_two_cam.GetComponent<AudioListener>().enabled = false;
            car_three_cam.GetComponent<AudioListener>().enabled = true;
            car_four_cam.GetComponent<AudioListener>().enabled = false;

            //On car 3 camera the information panel is visible
            info_panel.SetActive(true);
            //Change the panel title based on the car selected (Car 3)
            panel_title_text.text = "Information about Car 3";
        }
        else if (camera_index == 4)
        {
            //Enable car 4 camera and disable all the others
            free_look_cam.enabled = false;
            car_one_cam.enabled = false;
            car_two_cam.enabled = false;
            car_three_cam.enabled = false;
            car_four_cam.enabled = true;

            //Enable audio listener of the car 4 camera and disable all the others
            free_look_cam.GetComponent<AudioListener>().enabled = false;
            car_one_cam.GetComponent<AudioListener>().enabled = false;
            car_two_cam.GetComponent<AudioListener>().enabled = false;
            car_three_cam.GetComponent<AudioListener>().enabled = false;
            car_four_cam.GetComponent<AudioListener>().enabled = true;

            //On car 4 camera the information panel is visible
            info_panel.SetActive(true);
            //Change the panel title based on the car selected (Car 4)
            panel_title_text.text = "Information about Car 4";
        }
        //Change the camera text name best of the current selected camera index
        camera_name_text.text = cameras[camera_index];
    }
}
