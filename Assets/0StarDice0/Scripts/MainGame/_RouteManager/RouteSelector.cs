//// In RouteSelector.cs

//using System.Collections.Generic;
//using UnityEngine;

//public class RouteSelector : MonoBehaviour
//{
//    [Tooltip("เชื่อมโยงกับ RouteManager")]
//    public RouteManager routeManager;

//    /// <summary>
//    /// คืนค่าโหนดถัดไปถ้ามีทางเดียว หรือ null ถ้าไม่มีทางไปหรือมีหลายทางเลือก
//    /// </summary>
//    public Transform ChooseNextNode(Transform currentNode)
//    {
//        if (routeManager == null)
//        {
//            Debug.LogError("RouteManager is not assigned in RouteSelector.");
//            return null;
//        }

//        var connection = routeManager.nodeConnections.Find(nc => nc.node == currentNode);
//        if (connection == null || connection.connectedNodes.Count == 0)
//        {
//            return null; // ไม่มีโหนดเชื่อมต่อ
//        }

//        if (connection.connectedNodes.Count == 1)
//        {
//            return connection.connectedNodes[0]; // มีทางเลือกเดียว ให้ไปทางนั้น
//        }

//        // มีมากกว่า 1 ทางเลือก (เป็นทางแยก) คืนค่า null เพื่อให้ PlayerController จัดการเรื่องการเลือก
//        return null;
//    }

//    // คุณอาจจะมีเมธอดสำหรับดึง "ทุก" ทางเลือกสำหรับ UI โดยเฉพาะ ถ้าต้องการ
//    public List<Transform> GetAllConnectedNodes(Transform currentNode)
//    {
//        if (routeManager == null) return new List<Transform>();
//        var connection = routeManager.nodeConnections.Find(nc => nc.node == currentNode);
//        if (connection != null)
//        {
//            return connection.connectedNodes;
//        }
//        return new List<Transform>();
//    }
//}