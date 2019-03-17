using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    class Monster
    {
        private string nameMonster;
        private List<Monster> mnsterList = new List<Monster>();
      
        public void addMonster(string nameMonster)
        {
            Monster m = new Monster();
            m.nameMonster = nameMonster;
            mnsterList.Add(m);
        }

        public bool checkMonster(string nameMonster)
        {
            string[] a = nameMonster.Split(';');
            foreach (Monster monster in mnsterList)
            {
                for(int i = 0; i < a.Length; i++)
                {
                    if (monster.nameMonster == a[i])
                    {
                        return true;
                    }
                }
               
            }
            return false;
        }
    }
}
