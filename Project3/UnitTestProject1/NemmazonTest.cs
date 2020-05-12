using System;
using System.Collections.Generic;
using System.Linq;
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
        public void StepIncTest()
        {
            _model._kozpont.NewSimulation(data);

            _model.StepSimulation();

            Assert.AreEqual(1, _model._kozpont.TotalSteps);
        }

        [TestMethod]
        public void MoveNoPolcTest()
        {
            _model._kozpont.NewSimulation(data);

            _model.StepSimulation();

            Assert.AreEqual(null, _model._kozpont.robots[0].polc);


        }

        [TestMethod]
        public void MovePolcTest()
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
        public void EnergyConsumptionTest()
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
        public void TotalStepsTest()
        {
            _model._kozpont.NewSimulation(data);

            _model.StepSimulation();
            _model.StepSimulation();
            _model.StepSimulation();
            _model.StepSimulation();

            Assert.IsTrue(_model._kozpont.TotalSteps == 4);

        }

        [TestMethod]
        public void AddStopTest()
        {
            _model._kozpont.NewSimulation(data);

            _model.StepSimulation();
            _model.StepSimulation();
            _model.StepSimulation();
            _model.StepSimulation();

            _model._kozpont.paths[0].Clear();

            _model._kozpont.AddStop(_model._kozpont.robots[0], 1);

            int X, Y;
            X = _model._kozpont.robots[0].x;
            Y = _model._kozpont.robots[0].y;

            _model.StepSimulation();

            Assert.AreEqual(X, _model._kozpont.robots[0].x);
            Assert.AreEqual(Y, _model._kozpont.robots[0].y);

        }

        [TestMethod]
        public void CalculateNextJobTest()
        {
            _model._kozpont.NewSimulation(data);

            _model._kozpont.robots[0].energy = 15;
            NewmazonClasses test = _model._kozpont.CalculateNextJob(_model._kozpont.robots[0]);

            int id_kivonas = 10000; //ID alapján tesztelünk amelyek 10000-ként váltanak pl robot, végállomás, töltöállomás

            Assert.AreEqual(test.ID, _model._kozpont.robots[0].ID - id_kivonas);

        }

        [TestMethod]
        public void CalculateRouteTest()
        {
            _model._kozpont.NewSimulation(data);

            _model.StepSimulation();
            _model.StepSimulation();

            Assert.AreEqual(0, _model._kozpont.robots[0].x);
            Assert.AreEqual(1, _model._kozpont.robots[0].y);

            _model.StepSimulation();

            Assert.AreEqual(0, _model._kozpont.robots[0].x);
            Assert.AreEqual(2, _model._kozpont.robots[0].y);

            _model.StepSimulation();

            Assert.AreEqual(0, _model._kozpont.robots[0].x);
            Assert.AreEqual(2, _model._kozpont.robots[0].y);

            _model.StepSimulation();

            Assert.AreEqual(1, _model._kozpont.robots[0].x);
            Assert.AreEqual(2, _model._kozpont.robots[0].y);

        }

        [TestMethod]
        public void DoStationaryThingsTest() //polc felvétele
        {
            _model._kozpont.NewSimulation(data);

            _model._kozpont.robots[0].x = 1;
            _model._kozpont.robots[0].y = 2;

            Assert.AreEqual(null, _model._kozpont.robots[0].polc);

            _model._kozpont.DoStationaryThings(_model._kozpont.robots[0]);

            Assert.AreNotEqual(null, _model._kozpont.robots[0].polc);

        }

        [TestMethod]
        public void DoStationaryThingsTest2() //polc lerakása
        {
            _model._kozpont.NewSimulation(data);

            _model._kozpont.robots[0].polc = (Polc)_model._kozpont.table[1, 2];

            _model._kozpont.robots[0].polc.goods.Clear();

            Assert.AreNotEqual(null, _model._kozpont.robots[0].polc);

            _model._kozpont.robots[0].x = 1;
            _model._kozpont.robots[0].y = 2;

            _model._kozpont.DoStationaryThings(_model._kozpont.robots[0]);

            Assert.AreEqual(null, _model._kozpont.robots[0].polc);

        }

        [TestMethod]
        public void DoStationaryThingsTest3() //lepakol terméket a polcról
        {
            _model._kozpont.NewSimulation(data);

            _model._kozpont.robots[0].x = 1;
            _model._kozpont.robots[0].y = 2;

            Assert.AreEqual(null, _model._kozpont.robots[0].polc);

            _model._kozpont.DoStationaryThings(_model._kozpont.robots[0]);

            Assert.AreNotEqual(null, _model._kozpont.robots[0].polc);

            int count = _model._kozpont.robots[0].polc.goods.Count();

            _model._kozpont.robots[0].x = 4;
            _model._kozpont.robots[0].y = 2;

            _model._kozpont.DoStationaryThings(_model._kozpont.robots[0]);

            Assert.AreNotEqual(count, _model._kozpont.robots[0].polc.goods.Count());

        }

        [TestMethod]
        public void DoStationaryThingsTest4() //tölt
        {
            _model._kozpont.NewSimulation(data);

            _model._kozpont.robots[0].x = 2;
            _model._kozpont.robots[0].y = 4;

            _model._kozpont.robots[0].energy = 1;

            _model._kozpont.DoStationaryThings(_model._kozpont.robots[0]);

            Assert.AreNotEqual(1, _model._kozpont.robots[0].energy);

        }
    }
}
