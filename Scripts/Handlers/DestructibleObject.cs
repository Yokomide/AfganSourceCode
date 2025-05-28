using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DestructibleObject : MonoBehaviour
{
    private List<DestructiblePart> _destructibleParts = new List<DestructiblePart>();

    private void Awake()
    {
        _destructibleParts = GetComponentsInChildren<DestructiblePart>().ToList();
        for (int i = 0; i < _destructibleParts.Count; i++)
        {
            _destructibleParts[i].Initialize(this);
        }
    }
    
    public void DestroyObject()
    {
        foreach (var part in _destructibleParts)
        {
            if (!part.IsDestroyed)
            {
                part.DestroyPart();
            }
        }

        Destroy(gameObject, 0.1f);
    }
    
    public void OnKeyPartDestroyed(DestructiblePart keyPart)
    {
        bool allKeyPartsDestroyed = _destructibleParts
            .Where(p => p.IsKeyPart)
            .All(p => p.IsDestroyed);

        if (allKeyPartsDestroyed)
        {
            foreach (var part in _destructibleParts)
            {
                if(part == null)
                    continue;
                if (!part.IsDestroyed)
                {
                    part.DestroyPart();
                }
            }
        }
        else
        {
            foreach (var dependentPart in keyPart.DependentParts)
            {
                if(dependentPart == null)
                    continue;
                if (!dependentPart.IsDestroyed)
                {
                    dependentPart.DestroyPart();
                }
            }
        }
    }
}