using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.iOS;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace SaveMetaData
{

    // Classes to hold model information

    [System.Serializable]
    public class ModelInfo
    {
        public float px; //position.x
        public float py; //position.y 
        public float pz; //position.z 
        public float qx; //rotation.x 
        public float qy; //rotation.y 
        public float qz; //rotation.z 
        public float qw; //rotation.w 
        public int modelType;
    }

    [System.Serializable]
    public class ModelList
    {
        public ModelInfo[] models;
    }


     // Main Class for Managing Models

    public class ModelManager : MonoBehaviour
    {

        public GameObject[] modelPrefabs; // 3 prefabs are attached in the inspector

        public List<ModelInfo> ModelInfoList = new List<ModelInfo>();
        public List<GameObject> ModelObjList = new List<GameObject>();

        // Functions for adding and deleting models

        public void OnAddShapeClicked()
        {
            // generate random object type

            System.Random rnd = new System.Random();
            int type = rnd.Next(0, 3);

            Vector3 position = Camera.main.transform.position + Camera.main.transform.forward * 0.5f;
            Quaternion rotation = Camera.main.transform.rotation;

            // create model info object
            ModelInfo modelInfo = new ModelInfo();
            modelInfo.px = position.x;
            modelInfo.py = position.y;
            modelInfo.pz = position.z;
            modelInfo.qx = rotation.x;
            modelInfo.qy = rotation.y;
            modelInfo.qz = rotation.z;
            modelInfo.qw = rotation.w;
            modelInfo.modelType = type;
			//store a position and rotation into a class ref and pass that ref to a method to create object from the ref's data

            AddModel(modelInfo);

        }

        public void AddModel(ModelInfo modelInfo) //add model to temp list and scene
        {
            ModelInfoList.Add(modelInfo); //add the data to list of class data ref

            GameObject newModel = Instantiate(modelPrefabs[modelInfo.modelType]); //create random model from the prefab array

            newModel.transform.position = new Vector3(modelInfo.px, modelInfo.py, modelInfo.pz); //set position according to the parameter
            newModel.transform.rotation = new Quaternion(modelInfo.qx, modelInfo.qy, modelInfo.qz, modelInfo.qw); // set rotation according to the parameter

            ModelObjList.Add(newModel); //store the Created gameobject in the list
        }

        public void ClearModels() //destroy all created object in scene
        {
            foreach (var obj in ModelObjList)
            {
                Destroy(obj);
            }
            ModelObjList.Clear(); //as well as clear both info and GameObject list
            ModelInfoList.Clear(); //make sure to always save model to JSON before calling this function
        }

        // Helper Functions to convert models to and from JSON
        public JObject Models2JSON()
        {
            ModelList modelList = new ModelList();
            modelList.models = new ModelInfo[ModelInfoList.Count]; //create a new array of info from the temporary list count
            for (int i = 0; i < ModelInfoList.Count; i++)
            {
                modelList.models[i] = ModelInfoList[i]; 
            }

            return JObject.FromObject(modelList); //return json object created from the array
        }



        public void LoadModelsFromJSON(JToken mapMetadata)
        {
            ClearModels();

            if (mapMetadata is JObject && mapMetadata["modelList"] is JObject)
            {
                ModelList modelList = mapMetadata["modelList"].ToObject<ModelList>();
                if (modelList.models == null)
                {
                    Debug.Log("no models added");
                    return;
                }

                foreach (var modelInfo in modelList.models)
                {
                    AddModel(modelInfo);
                }
            }

        }


    }

}