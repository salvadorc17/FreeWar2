using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DEngine;


namespace FactionsGame
{
    /// <summary>
    /// An ant system agent which uses the ant graph and the ant system search variables
    /// to attempt to find the goal.
    /// </summary>
    public class Ant
    {
        protected AntsSystem antsSystem;

        protected float exploreValue = 1f;              /// Tendency for ants to be independent
        protected float exploitValue = 1f;              /// Tendency for ants to exploit existing pheromone trails
        protected float heuristicPressure = 4f;         /// Tendency for ants to obey the heuristic pressure towards the goal
        protected float momentumPressure = 2f;          /// Tendency for the ant to follow its existing momentum
        protected float pheromoneStrength = 1f;         /// Strength of pheromone laid down by an ant.


        private int distanceMoved = 0;
        Random randomNumber = new Random();
        GridReference startPoint;
        GridReference endPoint;
        GridReference currentLocation;
        GridReference lastLocation;



        #region Public Properties
        /// <summary>
        /// Tendency for ants to be independent
        /// </summary>
        public float ExploreValue
        {
            get
            {
                return exploreValue;
            }
            set
            {
                exploreValue = value;
            }
        }
        /// <summary>
        /// Tendency for ants to exploit existing pheromone trails
        /// </summary>
        public float ExploitValue
        {
            get
            {
                return exploitValue;
            }
            set
            {
                exploitValue = value;
            }
        }
        /// <summary>
        /// Tendency for ants to obey the heuristic pressure towards the goal
        /// </summary>
        public float HeuristicPressure
        {
            get
            {
                return heuristicPressure;
            }
            set
            {
                heuristicPressure = value;
            }
        }
        /// <summary>
        /// Tendency for the ant to follow its existing momentum
        /// </summary>
        public float Momentum
        {
            get
            {
                return momentumPressure;
            }
            set
            {
                momentumPressure = value;
            }
        }
        /// <summary>
        /// Strength of pheromone laid down by an ant.
        /// </summary>
        public float PheromoneStrength
        {
            get
            {
                return pheromoneStrength;
            }
            set
            {
                pheromoneStrength = value;
            }
        }
        #endregion


        public Ant(AntsSystem _antsSystem)
        {
            antsSystem = _antsSystem;
            startPoint = antsSystem.StartLocation;
            endPoint = antsSystem.EndLocation;
            currentLocation = startPoint;
            lastLocation = currentLocation;
        }


        /// <summary>
        /// Release the hound!
        /// Let the ant loose upon the tile grid, and deposit pheromones as necessary.
        /// </summary>
        public Collection<AntGraphEdge> AntSearch()
        {
            // The path this ant traverses
            Collection<AntGraphEdge> antPath = new Collection<AntGraphEdge>();
            Collection<AntGraphEdge> tabuList = new Collection<AntGraphEdge>();


            // If our path is colossal, greatly increase heuristic pressure.
            // Decrease load time at the cost of "creativity"
            if (antsSystem.MaxAntPathLength > 250)
            {
                heuristicPressure = 25;
            }


            // Explore until we expire or we've found the goal
            bool goalFound = false;
            while (distanceMoved < antsSystem.MaxAntPathLength && !goalFound)
            {
                // Heuristic values!
                // ------------------

                // Get heuristically-ranked edges for the current location
                Collection<AntGraphEdge> rankedEdges = antsSystem.AntGraph[currentLocation.X, currentLocation.Y].RankedEdges;
                if (rankedEdges.Count == 0)
                {
                    // We have come to an invalid point with no non-solid neighbours (or neighbour tiles not found!)
                    //Log.Message("Ant has come to an invalid point with no non-solid neighbours (or neighbour tiles not found!)");
                    antPath.Clear();
                    break;
                }

                // Remove tabu list items
                for (int i = 0; i < rankedEdges.Count; i++)
                {
                    if (tabuList.Contains(rankedEdges[i]))
                    {
                        rankedEdges.Remove(rankedEdges[i]);
                        i--;
                    }
                }

                // If we've trapped ourselves and run out of edges, clear the tabu list and continue.
                if (rankedEdges.Count == 0)
                {
                    tabuList.Clear();
                    rankedEdges = antsSystem.AntGraph[currentLocation.X, currentLocation.Y].RankedEdges;
                }

                // Get ant based heuristic value
                Collection<float> heuristicValues = new Collection<float>();
                foreach (AntGraphEdge rankedEdge in rankedEdges)
                {
                    // AntBasedPathfinding.pdf: page 34
                    // H = ((sum(edges) - rank(edge) + 1) / sum(edges))^heuristicPressure
                    float heuristic = (rankedEdges.Count - rankedEdges.IndexOf(rankedEdge)) + 1 / (rankedEdges.Count);
                    heuristic = (float)Math.Pow(heuristic, heuristicPressure);
                    heuristicValues.Add(heuristic);
                }


                // Momentum values!
                // ------------------

                // Get relative distance between the end of this edge and the last location.
                Collection<float> relativeMoveDistances = new Collection<float>();
                foreach (AntGraphEdge rankedEdge in rankedEdges)
                {
                    GridReference relativeDist = rankedEdge.targetRef - lastLocation;
                    relativeMoveDistances.Add((float)Math.Sqrt(Math.Pow(relativeDist.X, 2) + Math.Pow(relativeDist.Y,2)));
                }

                // Rank the relative distances
                // Get an index order
                Collection<int> rankedMomentumValueIndexes = new Collection<int>();
                while (rankedMomentumValueIndexes.Count < relativeMoveDistances.Count)
                {
                    float leastDistance = -1f;
                    int lowestIndex = -1;
                    for (int i = 0; i < relativeMoveDistances.Count; i++)
                    {
                        // Exclude already used
                        if (rankedMomentumValueIndexes.Contains(i))
                            continue;

                        if (leastDistance == -1f || relativeMoveDistances[i] < leastDistance)
                        {
                            leastDistance = relativeMoveDistances[i];
                            lowestIndex = i;
                        }
                    }

                    if (lowestIndex > -1)
                    {
                        rankedMomentumValueIndexes.Add(lowestIndex);
                    }
                }


                // Obtain ant system values for momentum
                // AntBasedPathfinding.pdf page 34
                Collection<float> rankedMomentumValues = new Collection<float>();
                for (int i = 0; i < rankedMomentumValueIndexes.Count; i++)
                {
                    float momentumValue = (rankedMomentumValueIndexes.Count - rankedMomentumValueIndexes.IndexOf(i)) + 1;
                    momentumValue /= rankedMomentumValueIndexes.Count;
                    momentumValue = (float)Math.Pow(momentumValue, momentumPressure);
                    rankedMomentumValues.Add(momentumValue);
                }


                // Overall cost & probability values
                // ----------------------------

                // Get cost for each edge & total cost
                float totalEdgeCost = 0f;
                Collection<float> edgeCosts = new Collection<float>();
                for (int i = 0; i < rankedEdges.Count; i++)
                {
                    // Get the pheromone value
                    float edgePheromoneValue = rankedEdges[i].PheromoneValue;

                    // Calculate explore/exploit value using pheromone value
                    float exploreExploitValue = (float)Math.Pow(exploreValue + edgePheromoneValue, exploitValue);

                    // Calc total edge cost (T + H + M)
                    float edgeCost = exploreExploitValue + heuristicValues[i] + rankedMomentumValues[i];

                    edgeCosts.Add(edgeCost);
                    totalEdgeCost += edgeCost;
                }


                // Overall probabilities of taking each edge
                Collection<float> edgeProbabilities = new Collection<float>();
                float probabilityTotal = 0f;
                for (int i = 0; i < edgeCosts.Count; i++)
                {
                    // Calculate overall probability of this edge and add to edge probabilities
                    float edgeProbability = edgeCosts[i] / totalEdgeCost;
                    edgeProbabilities.Add(edgeProbability);

                    // Add to total probability of all edges
                    probabilityTotal += edgeProbability;
                }


                // Generate a random number between 0 and the probability total!
                float randomProbabilityValue = (float)randomNumber.NextDouble();

                // Iterate through probabilities and discover where the random number lies.
                // Keep the index of this edge
                float probabilitySum = 0f;
                int targetEdgeIndex = -1;
                for (int i = 0; i < edgeProbabilities.Count; i++)
                {
                    if (randomProbabilityValue >= probabilitySum &&
                        randomProbabilityValue < (probabilitySum + edgeProbabilities[i]))
                    {
                        targetEdgeIndex = i;
                        break;
                    }
                    probabilitySum += edgeProbabilities[i];
                }


                // Pick this edge and traverse to its target!
                if (targetEdgeIndex >= 0)
                {
                    AntGraphEdge targetEdge = rankedEdges[targetEdgeIndex];
                    lastLocation = currentLocation;         // Update last location
                    currentLocation = targetEdge.targetRef; // Update current location
                    antPath.Add(targetEdge);            // Add to the chosen path
                    tabuList.Add(targetEdge);           // Make sure it cant traverse this again
                    distanceMoved++;                    // Increment distance

                    // Break if it's the end point
                    if (targetEdge.targetRef == endPoint)
                    {
                        goalFound = true;
                        break;
                    }
                }
            }
            return antPath;
        }



    }
}

