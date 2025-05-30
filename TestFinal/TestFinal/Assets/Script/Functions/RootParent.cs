using UnityEngine;

public static class RootParent{

    public static GameObject GetRootParent(GameObject obj){
        Transform currentTransform = obj.transform;
        
        while (currentTransform.parent != null){
            currentTransform = currentTransform.parent;
        }

        return currentTransform.gameObject;
    }
}