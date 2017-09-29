using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBikes.DomainLogic.StandardLibrary
{
    public class Rider
    {
        public int CurrentHitPoints { get; private set; }

        public Rider(int hitPoints)
        {
            CurrentHitPoints = hitPoints;
        }

        public void ReceiveDamage(int hitPointsOfDamage)
        {
            CurrentHitPoints = (CurrentHitPoints - hitPointsOfDamage);
        }
    }
}
