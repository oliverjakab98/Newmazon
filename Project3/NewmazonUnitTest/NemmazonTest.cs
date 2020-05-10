using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newmazon.Model;
using Newmazon.Persistence;

namespace NewmazonUnitTest
{
    [TestClass]
    public class NemmazonTest
    {
        private NewmazonModel _model;
        private AllData data;

        
        [TestInitialize]
        public void InInitialize()
        {

            char[,] dt = { {'R', 'M', 'M', 'M', 'M' },
                           {'F', 'M', 'P', 'M', 'M' },
                           {'F', 'M', 'P', 'M', 'T' },
                           {'F', 'M', 'M', 'M', 'M' },
                           {'F', 'M', 'C', 'M', 'C' } };
            List<Goods> g = new List<Goods>();
            int[] g1 = new int[1]; g1[0] = 1;
            int[] g2 = new int[2]; g2[0] = 1; g2[1] = 2;
            g.Add(new Goods(1, 2, g1));
            g.Add(new Goods(2, 2, g2));
            int tS = 5;
            int rE = 100;
            data = new AllData(dt, g, tS, rE);


            _model = new NewmazonModel(null);
            
        }

        [TestMethod]
        public void TestStepInc()
        {
            _model._kozpont.NewSimulation(data);

            _model.StepSimulation();

            Assert.AreEqual(1, _model._kozpont.TotalSteps);
        }

        [TestMethod]
        public void TestMoveNoPolc() 
        {
            _model._kozpont.NewSimulation(data);

            _model.StepSimulation();

            Assert.AreEqual(null, _model._kozpont.robots[0].polc);

            
        }

        [TestMethod]
        public void TestMovePolc()
        {
            _model._kozpont.NewSimulation(data);

            _model.StepSimulation();

            Assert.AreEqual(null, _model._kozpont.robots[0].polc);

            _model.StepSimulation();
            _model.StepSimulation();
            _model.StepSimulation();
            _model.StepSimulation();
            _model.StepSimulation();
            _model.StepSimulation();

            Assert.AreNotEqual(null, _model._kozpont.robots[0].polc);
        }

        [TestMethod]
        public void TestEnergyConsumption()
        {
            _model._kozpont.NewSimulation(data);

            int energy = _model._kozpont.robots[0].energy;

            _model.StepSimulation();
            _model.StepSimulation();
            _model.StepSimulation();
            _model.StepSimulation();

            Assert.AreNotEqual(energy, _model._kozpont.robots[0].energy);

           
        }

        [TestMethod]
        public void TotalSteps()
        {
            _model._kozpont.NewSimulation(data);

            _model.StepSimulation();
            _model.StepSimulation();
            _model.StepSimulation();
            _model.StepSimulation();

            Assert.IsTrue(_model._kozpont.TotalSteps == 4);


        }
    }
}
