using UnityEngine;
using System.Collections.Generic;
using Zenject;
using System.Linq;

public class HexMeshCombiner : BaseSignal
{
    [Inject] Pool Pool;

    [SerializeField, ReadOnly] List<MeshFilter> OriginalHexFilters = new List<MeshFilter>();
    [SerializeField, ReadOnly] List<Hex> HiddenHexes = new List<Hex>();

    private GameObject CombinedMeshObject;

    void Start()
    {
        var UnWalkableHexes = Pool.GetAllOfType<Hex>().Where(x => !x.IsWalkable).ToHashSet();

        foreach (Hex hex in UnWalkableHexes)
        {
            MeshFilter meshFilter = hex.HexVisual.GetComponent<MeshFilter>();

            if (meshFilter != null)
            {
                OriginalHexFilters.Add(meshFilter);
                HiddenHexes.Add(hex);
            }
        }

        if (OriginalHexFilters.Count > 0)
        {
            CombineMeshes();

            foreach (var hex in HiddenHexes)
            {
                hex.gameObject.SetActive(false);
            }
        }
    }

    void OnDestroy()
    {
        if (CombinedMeshObject != null)
        {
            Destroy(CombinedMeshObject);

            foreach (var hex in HiddenHexes)
            {
                hex.gameObject.SetActive(true);
            }
        }
    }

    void CombineMeshes()
    {
        CombineInstance[] combine = new CombineInstance[OriginalHexFilters.Count];

        for (int i = 0; i < OriginalHexFilters.Count; i++)
        {
            combine[i].mesh = OriginalHexFilters[i].sharedMesh;
            combine[i].transform = OriginalHexFilters[i].transform.localToWorldMatrix;
        }

        CombinedMeshObject = new GameObject("CombinedHexMesh");
        MeshFilter combinedMeshFilter = CombinedMeshObject.AddComponent<MeshFilter>();
        MeshRenderer combinedMeshRenderer = CombinedMeshObject.AddComponent<MeshRenderer>();

        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combine);
        combinedMeshFilter.mesh = combinedMesh;

        combinedMeshRenderer.material = OriginalHexFilters[0].GetComponent<MeshRenderer>().material;

        EmitSignal(new Message(gameObject, $"Combined meshes: {OriginalHexFilters.Count}"));
    }
}
