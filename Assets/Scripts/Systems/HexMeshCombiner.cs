using UnityEngine;
using System.Collections.Generic;
using Zenject;
using System.Linq;

public class HexMeshCombiner : BaseSignal
{
    [Inject] Pool Pool;

    [SerializeField, ReadOnly] List<MeshFilter> OriginalHexFilters = new List<MeshFilter>();
    [SerializeField, ReadOnly] List<Hex> HiddenHexes = new List<Hex>();

    [SerializeField] Material OutlineMaterial;

    private GameObject CombineParent;

    private List<GameObject> CombinedMeshObjects = new List<GameObject>();

    void Start()
    {
        CombineParent = new GameObject("CombineMesh");
        CombineParent.transform.SetParent(transform);

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
            CombineMeshesByMaterial();

            foreach (Hex Hex in HiddenHexes)
            {
                Hex.gameObject.SetActive(false);
            }
        }
    }

    void OnDestroy()
    {
        foreach (GameObject CombinedMeshObject in CombinedMeshObjects)
        {
            if (CombinedMeshObject != null)
            {
                Destroy(CombinedMeshObject);
            }
        }

        foreach (Hex Hex in HiddenHexes)
        {
            if (Hex != null)
                Hex.gameObject.SetActive(true);
        }
    }

    void CombineMeshesByMaterial()
    {
        var MaterialGroups = OriginalHexFilters
            .GroupBy(Filter => Filter.GetComponent<MeshRenderer>().sharedMaterial)
            .ToDictionary(Group => Group.Key, Group => Group.ToList());

        foreach (var MaterialGroup in MaterialGroups)
        {
            CombineInstance[] Combine = new CombineInstance[MaterialGroup.Value.Count];

            for (int I = 0; I < MaterialGroup.Value.Count; I++)
            {
                Combine[I].mesh = MaterialGroup.Value[I].sharedMesh;
                Combine[I].transform = MaterialGroup.Value[I].transform.localToWorldMatrix;
            }

            GameObject CombinedMeshObject = new GameObject($"CombinedHexMesh_{MaterialGroup.Key.name}");
            CombinedMeshObject.transform.SetParent(CombineParent.transform);

            MeshFilter CombinedMeshFilter = CombinedMeshObject.AddComponent<MeshFilter>();
            MeshRenderer CombinedMeshRenderer = CombinedMeshObject.AddComponent<MeshRenderer>();

            Mesh CombinedMesh = new Mesh();
            CombinedMesh.CombineMeshes(Combine);
            CombinedMeshFilter.mesh = CombinedMesh;

            if (OutlineMaterial != null)
            {
                CombinedMeshRenderer.materials = new Material[] { MaterialGroup.Key, OutlineMaterial };
            }
            else
            {
                CombinedMeshRenderer.material = MaterialGroup.Key;
            }

            CombinedMeshObject.isStatic = true;
            CombinedMeshObjects.Add(CombinedMeshObject);

            EmitSignal(new Message(gameObject, $"Combined meshes for material {MaterialGroup.Key.name}: {MaterialGroup.Value.Count}"));
        }
    }
}