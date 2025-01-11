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
        var UnWalkableHexes = Pool.GetAllOfType<Hex>().Where(X => !X.IsWalkable).ToHashSet();

        foreach (Hex Hex in UnWalkableHexes)
        {
            MeshFilter MeshFilter = Hex.HexVisual.GetComponent<MeshFilter>();

            if (MeshFilter != null)
            {
                OriginalHexFilters.Add(MeshFilter);
                HiddenHexes.Add(Hex);
            }
        }

        if (OriginalHexFilters.Count > 0)
        {
            CombineMeshes();

            foreach (var Hex in HiddenHexes)
            {
                Hex.gameObject.SetActive(false);
            }
        }
    }

    void OnDestroy()
    {
        if (CombinedMeshObject != null)
        {
            Destroy(CombinedMeshObject);

            foreach (var Hex in HiddenHexes)
            {
                Hex.gameObject.SetActive(true);
            }
        }
    }

    void CombineMeshes()
    {
        CombineInstance[] Combine = new CombineInstance[OriginalHexFilters.Count];

        for (int I = 0; I < OriginalHexFilters.Count; I++)
        {
            Combine[I].mesh = OriginalHexFilters[I].sharedMesh;
            Combine[I].transform = OriginalHexFilters[I].transform.localToWorldMatrix;
        }

        CombinedMeshObject = new GameObject("CombinedHexMesh");
        MeshFilter CombinedMeshFilter = CombinedMeshObject.AddComponent<MeshFilter>();
        MeshRenderer CombinedMeshRenderer = CombinedMeshObject.AddComponent<MeshRenderer>();

        Mesh CombinedMesh = new Mesh();
        CombinedMesh.CombineMeshes(Combine);
        CombinedMeshFilter.mesh = CombinedMesh;

        CombinedMeshRenderer.material = OriginalHexFilters[0].GetComponent<MeshRenderer>().material;

        EmitSignal(new Message(gameObject, $"Combined meshes: {OriginalHexFilters.Count}"));
    }
}