using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MathNet.Numerics.LinearAlgebra;
using Matrix = MathNet.Numerics.LinearAlgebra.Matrix<float>;
using System;
using System.IO;
using Newtonsoft.Json;
using Random = UnityEngine.Random;

public class NNet : MonoBehaviour
{
    public Matrix<float> inputLayer = Matrix<float>.Build.Dense(1, 3);

    public List<Matrix<float>> hiddenLayers = new List<Matrix<float>>();

    public Matrix<float> outputLayer = Matrix<float>.Build.Dense(1, 2);

    public List<Matrix<float>> weights = new List<Matrix<float>>();

    public List<float> biases = new List<float>();

    public float fitness;


    public void init (int hiddenLayerCount , int hiddenNeuronCount)
    {
        //In case we init in the middle of the program we make sure to clear
        //all the matrices and lists
        inputLayer.Clear();
        hiddenLayers.Clear();
        outputLayer.Clear();
        weights.Clear();
        biases.Clear();

        //For each hidden layer of the Neural Network
        for (int i = 0; i < hiddenLayerCount + 1; i++)
        {
            //Build a matrix of one row and total hidden layer neurons count columns
            Matrix<float> f = Matrix<float>.Build.Dense(1, hiddenNeuronCount);

            //add the matrix to the hidden layers list
            hiddenLayers.Add(f);

            //add a random bias with a random value between -1f and 1f
            biases.Add(Random.Range(-1f, 1f));

            //if i == 0 then we are at the first hidden layer of the NN so build the matrices that represent
            //the connection between the input layer and the first hidden layer and add it to the weights list
            if(i == 0)
            {
                //3 rows matrix because the input has 3 total inputs (sensor a , sensor b and sensor c)
                Matrix<float> inputToH1 = Matrix<float>.Build.Dense(3, hiddenNeuronCount);
                weights.Add(inputToH1);
            }

            //For the weights of the connection between the hidden layers we need to create a matrix that
            //has a row and column count equals to hidden neurons count and then add it to the weights list
            Matrix<float> HiddenToHidden = Matrix<float>.Build.Dense(hiddenNeuronCount, hiddenNeuronCount);
            weights.Add(HiddenToHidden);
        }
        //For the weights of the connection between output layer and last hidden layer is a matrix with 
        //hidden neurons count rows and 2 total columns (because the output layer has 2 neurons that 
        //predict the acceleration and turning of the car).
        Matrix<float> OutputWeight = Matrix<float>.Build.Dense(hiddenNeuronCount, 2);
        weights.Add(OutputWeight);
        //add a random bias with a random value between -1f and 1f
        biases.Add(Random.Range(-1f, 1f));

        //After creating all the weights matrices for all the conections between the layers
        //of the neural network call a function to randomize their values
        RandomiseWeights();
    }

    //Function that returns a copy of the class that it is in
    //From the class we only need the weights , the biases and the hidden layers
    public NNet InitialiseCopy (int hiddenLayerCount,int hiddenNeuronCount)
    {
        //Init the copy variable
        NNet n = new NNet();

        //Init a new Weights matrix
        List<Matrix<float>> newWeights = new List<Matrix<float>>();

        //Fill the matrix based of the current weights matrix
        for(int i=0; i < this.weights.Count; i++)
        {
            Matrix<float> currentWeight = Matrix<float>.Build.Dense(weights[i].RowCount, weights[i].ColumnCount);

            for(int x = 0; x < currentWeight.RowCount; x++)
            {
                for(int y=0; y < currentWeight.ColumnCount; y++)
                {
                    currentWeight[x, y] = weights[i][x, y];
                }
            }
            newWeights.Add(currentWeight);
        }

        //Init a new biases list
        List<float> newBiases = new List<float>();

        //Fill the new list with what the current biases list contains
        newBiases.AddRange(biases);

        n.weights = newWeights;
        n.biases = newBiases;

        //Initialise the new Neural Network reference hidden layers
        n.InitialiseHidden(hiddenLayerCount, hiddenNeuronCount);

        return n;
    }

    public void InitialiseHidden (int hiddenLayerCount , int hiddenNeuronCount)
    {
        //In case we init in the middle of the program we make sure to clear
        //all the matrices and lists
        inputLayer.Clear();
        hiddenLayers.Clear();
        outputLayer.Clear();

        //For each hidden layer create a matrix that represent that hidden layer
        for(int i=0; i<hiddenLayerCount+1; i++)
        {
            //The matrix has 1 row and total neurons count of the hidden layer columns
            Matrix<float> newHiddenLayer = Matrix<float>.Build.Dense(1, hiddenNeuronCount);
            hiddenLayers.Add(newHiddenLayer);
        }
    }

    //A function that for each weight matrix in the weights list randomize each of their
    //[row,column] with a random value between -1.0f and 1.0f
    public void RandomiseWeights()
    {
        for (int i=0; i < weights.Count; i++)
        {
            for (int k = 0; k < weights[i].RowCount; k++)
            {
                for (int y = 0; y < weights[i].ColumnCount; y++)
                {
                    weights[i][k, y] = Random.Range(-1f, 1f);
                }
            }
        }
    }

    //Function that runs the neural netowrk and returns the predicted acceleration and turn value of the NN
    //The inputs are the values of the three sensors of the car
    //a = sensor a value (right sensor of the car)
    //b = sensor b value (front sensor of the car)
    //c = sensor c value (left sensor of the car)
    public (float,float) RunNetwork(float a , float b , float c)
    {
        //Give the inputs to each of the three neurons of the input layer
        inputLayer[0, 0] = a;
        inputLayer[0, 1] = b;
        inputLayer[0, 2] = c;

        //Using point wise tahn instead of sigmoid activation function because we want values between -1 and 1
        //(acceleration is a value between -1 and 1)
        inputLayer = inputLayer.PointwiseTanh();

        //Find the values for the first hidden layer
        hiddenLayers[0] = ((inputLayer * weights[0]) + biases[0]).PointwiseTanh();

        //Loop and find the values for the rest hidden layers of the neural network
        for (int i = 1; i < hiddenLayers.Count; i++)
        {
            hiddenLayers[i] = ((hiddenLayers[i-1] * weights[i]) + biases[i]).PointwiseTanh();
        }

        //Find the values of the output layer given the values of the last hidden layer
        outputLayer = ((hiddenLayers[hiddenLayers.Count-1]*weights[weights.Count-1])+biases[biases.Count-1]).PointwiseTanh();

        //First output is acceleration and second output is streering
        return (Sigmoid(outputLayer[0,0]), (float)Math.Tanh(outputLayer[0,1]));
    }

    //Return the sigmoid activation function result 
    //given a float s (the s will be the predicted acceleration value by
    //neural network output)
    public float Sigmoid (float s)
    {
        return (1 / (1 + Mathf.Exp(-s)));
    }


    //Save the neural network by saving the input layer , hidden layer , output layer , weights and biases matrices
    //in txt files with a json format in a folder called SavedBrains in the desktop of the computer
    public void SaveNetwork()
    {
        string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        if (!System.IO.Directory.Exists(path + "\\SavedBrains"))
        {
            Directory.CreateDirectory(path + "\\SavedBrains");
        }
        path = path + "\\SavedBrains";
        string ID = DateTime.Now.Ticks.ToString();
        string json = JsonConvert.SerializeObject(inputLayer.ToArray());
        File.WriteAllText(path + "\\inputLayer_" + ID + ".txt", json);
        json = JsonConvert.SerializeObject(hiddenLayers.ToArray());
        File.WriteAllText(path + "\\hiddenLayers_" + ID + ".txt", json);
        json = JsonConvert.SerializeObject(outputLayer.ToArray());
        File.WriteAllText(path + "\\outputLayers_" + ID + ".txt", json);
        json = JsonConvert.SerializeObject(weights.ToArray());
        File.WriteAllText(path + "\\weights_" + ID + ".txt", json);
        json = JsonConvert.SerializeObject(biases.ToArray());
        File.WriteAllText(path + "\\biases_" + ID + ".txt", json);
    }
}
