using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Test_Taste_Console_Application.Domain.DataTransferObjects;

namespace Test_Taste_Console_Application.Domain.Objects
{
    public class Planet
    {
        public string Id { get; set; }
        public float SemiMajorAxis { get; set; }
        public ICollection<Moon> Moons { get; set; }
        public float AverageMoonGravity
        {
            get
            {
                if (Moons == null || Moons.Count == 0)
                {
                    // Handle the case where there are no moons
                    return 0.0f;
                }

                float totalGravity = 0.0f;

                foreach (var moon in Moons)
                {
                    totalGravity += moon.Gravity;
                }

                return totalGravity / Moons.Count;
            }
        }

        // Exercise 2

        //Benefit: This alternative solution uses LINQ's Select and Average methods, providing a concise and expressive way to calculate the average moon gravity.
        //Drawback: While concise and readable, this approach might have a slightly higher performance overhead compared to the explicit loop in the original solution, particularly for large collections, due to the additional abstraction introduced by LINQ.

        //public float AverageMoonGravity => HasMoons() ? Moons.Select(moon => moon.Gravity).Average() : 0.0f;

        public Planet(PlanetDto planetDto)
        {
            Id = planetDto.Id;
            SemiMajorAxis = planetDto.SemiMajorAxis;
            Moons = new Collection<Moon>();
            if(planetDto.Moons != null)
            {
                foreach (MoonDto moonDto in planetDto.Moons)
                {
                    Moons.Add(new Moon(moonDto));
                }
            }
        }

        public Boolean HasMoons()
        {
            return (Moons != null && Moons.Count > 0);
        }
    }
}
