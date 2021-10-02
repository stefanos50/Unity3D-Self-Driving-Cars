using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MathNet.Numerics.LinearAlgebra;

public class GeneticManager : MonoBehaviour
{
    [Header("References")]
    public CarController controller;

    [Header("Controls")]
    public int initialPopulation = 85;
    [Range(0.0f, 1.0f)]
    public float mutationRate = 0.055f;

    [Header("Crossover Controls")]
    public int bestAgentSelection = 8;
    public int worstAgentSelection = 3;
    public int numberToCrossover;

    private List<int> genePool = new List<int>();

    private int naturallySelected;

    private NNet[] population;

    [Header("Public View")]
    public int currentGeneration;
    public int currentGenome;

    //At the start we want to completly randomize the population
    private void Start()
    {
        CreatePopulation();
    }

    //Randomize the population
    private void CreatePopulation()
    {
        population = new NNet[initialPopulation];
        FillPopulationWithRandomValues(population, 0);
        ResetToCurrentGenome();
    }

    //We pass a network to the ResetWithNetwork function of the 
    //CarController class so it can run it
    private void ResetToCurrentGenome()
    {
        controller.ResetWithNetwork(population[currentGenome]);
    }

    //startingIndex exists in the case we want to set only a specific amount of the population
    private void FillPopulationWithRandomValues(NNet[] newPopulation, int startingIndex)
    {
        while (startingIndex < initialPopulation)
        {
            //create a new Neural Network
            newPopulation[startingIndex] = new NNet();
            //Initialize it by creating the neural network matrices and randomize their values
            newPopulation[startingIndex].init(controller.LAYERS, controller.NEURONS);
            startingIndex++;
        }
    }

    //Function that is called every time the car crashes to a wall (goes out of the road)
    public void Death(float fitness, NNet network)
    {
        //If the current genome is lower than the total population then increase the current genome
        if(currentGenome < population.Length - 1)
        {
            population[currentGenome].fitness = fitness;
            currentGenome++;
            ResetToCurrentGenome();
        }else
        {
            RePopulate();
        }
    }

    private void RePopulate()
    {
        genePool.Clear();
        //On repopulation we go to a new generation
        currentGeneration++;
        naturallySelected = 0;

        //Sort the population by the fitness value (higher to lower)
        SortPopulation();

        //After the sorting pick the best
        NNet[] newPopulation = PickBestPopulation();

        Crossover(newPopulation);
        Mutate(newPopulation);

        FillPopulationWithRandomValues(newPopulation, naturallySelected);

        population = newPopulation;

        currentGenome = 0;
        ResetToCurrentGenome();
    }

    private void Mutate (NNet[] newPopulation)
    {
        //We only loop through the naturallySelected from the newPopulation because all the others
        //have not yet initialized
        for(int i = 0; i<naturallySelected; i++)
        {
            //for each element of the new population we will loop from its weights
            for (int c=0; c < newPopulation[i].weights.Count; c++)
            {
                //Add some randomness (the lower the mutationRate the lower the chance of this actually happening)
                if(Random.Range(0.0f,1.0f) < mutationRate)
                {
                    newPopulation[i].weights[c] = MutateMatrix(newPopulation[i].weights[c]);
                }
            }
        }
    }

    //A function that takes a matrix , randomize it and return it back
    Matrix<float> MutateMatrix(Matrix<float> A)
    {
        //We dont want to mutate all the values of the matrix but just a couple
        int randomPoints = Random.Range(1, (A.RowCount * A.ColumnCount) / 7);

        Matrix<float> C = A;

        for (int i = 0; i < randomPoints; i++)
        {
            //Get a random row,column index of the array
            int randomColumn = Random.Range(0, C.ColumnCount);
            int randomRow = Random.Range(0, C.RowCount);

            //Change the value of that random index by bumping the value a bit up or down instead of completly randomly changing it
            C[randomRow, randomColumn] = Mathf.Clamp(C[randomRow, randomColumn] + Random.Range(-1f, 1f), -1f, 1f);
        }

        return C;
    }

    private void Crossover(NNet[] newPopulation)
    {
        for (int i=0; i < numberToCrossover; i += 2)
        {
            //AIndex is the index of the first parent
            //BIndex is the index of the second parent
            int AIndex = i;
            int BIndex = i + 1;

            if(genePool.Count >= 1)
            {
                for(int l=0; l<100; l++)
                {
                    //Get a random index for the genePool
                    AIndex = genePool[Random.Range(0, genePool.Count)];
                    BIndex = genePool[Random.Range(0, genePool.Count)];

                    //AIndex and BIndex cannot get the same index from the genePool
                    if(AIndex != BIndex)
                    {
                        break;
                    }
                }
            }

            //Create the childs
            NNet Child1 = new NNet();
            NNet Child2 = new NNet();

            //Create a neural netowrk with random values for each child
            Child1.init(controller.LAYERS, controller.NEURONS);
            Child2.init(controller.LAYERS, controller.NEURONS);

            Child1.fitness = 0;
            Child2.fitness = 0;

            for(int w=0; w < Child1.weights.Count; w++)
            {
                //50% chance going to the if statement and 50% chance to go to the else statement
                //In the if statement the Child 1 will get its weights from the AIndex and Child 2 from
                //BIndex while in else statement the Child 1 from the BIndex and Child2 from the AIndex
                if(Random.Range(0.0f,1.0f) < 0.5f)
                {
                    Child1.weights[w] = population[AIndex].weights[w];
                    Child2.weights[w] = population[BIndex].weights[w];
                }
                else
                {
                    Child2.weights[w] = population[AIndex].weights[w];
                    Child1.weights[w] = population[BIndex].weights[w];
                }
            }

            for (int w = 0; w < Child1.biases.Count; w++)
            {
                //50% chance going to the if statement and 50% chance to go to the else statement
                //In the if statement the Child 1 will get its biases from the AIndex and Child 2 from
                //BIndex while in else statement the Child 1 from the BIndex and Child2 from the AIndex
                if (Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    Child1.biases[w] = population[AIndex].biases[w];
                    Child2.biases[w] = population[BIndex].biases[w];
                }
                else
                {
                    Child2.biases[w] = population[AIndex].biases[w];
                    Child1.biases[w] = population[BIndex].biases[w];
                }
            }

            newPopulation[naturallySelected] = Child1;
            naturallySelected++;

            newPopulation[naturallySelected] = Child2;
            naturallySelected++;
        }
    }


    private NNet[] PickBestPopulation()
    {
        NNet[] newPopulation = new NNet[initialPopulation];

        //First for loop is to select the best agents
        for(int i=0; i < bestAgentSelection; i++)
        {
            newPopulation[naturallySelected] = population[i].InitialiseCopy(controller.LAYERS,controller.NEURONS);
            newPopulation[naturallySelected].fitness = 0;
            naturallySelected++;
           
            //variable f is how many times we will add this current network to the gene pool
            int f = Mathf.RoundToInt(population[i].fitness * 10);


            for(int c=0; c < f+1; c++)
            {
                //adding index i to reference the neural networks
                genePool.Add(i);
            }
        }

        //Second for loop is to select the worst agents
        for (int i=0; i < worstAgentSelection; i++)
        {
            //inverse looping
            int last = population.Length - 1;
            last -= i;

            //variable f is how many times we will add this current network to the gene pool
            int f = Mathf.RoundToInt(population[last].fitness * 10);

            for (int c = 0; c < f + 1; c++)
            {
                //adding index last to reference the neural networks
                genePool.Add(last);
            }
        }
        return newPopulation;
    }

    //Sort the population based on the fitness value from the higher value
    //to the lower using a simple bubble short algorithm
    private void SortPopulation()
    {
        for(int i = 0; i < population.Length; i++)
        {
            for(int j = i; j < population.Length; j++)
            {
                if(population[i].fitness < population[j].fitness)
                {
                    NNet temp = population[i];
                    population[i] = population[j];
                    population[j] = temp;
                }
            }
        }
    }
}
