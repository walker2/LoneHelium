using UnityEngine;

public static class FurnitureActions
{
    public static void Door_UpdateAction(Furniture furniture, float deltaTime)
    {
        if (furniture.FurnParameters["isOpening"] >= 1)
        {
            furniture.FurnParameters["openness"] += deltaTime * 4; // TODO: Door open speed param
            if (furniture.FurnParameters["openness"] >= 1)
            {
                furniture.FurnParameters["isOpening"] = 0;
            }
        }
        else
        {
            furniture.FurnParameters["openness"] -= deltaTime * 4;
        }

        furniture.FurnParameters["openness"] = Mathf.Clamp01(furniture.FurnParameters["openness"]);
        furniture.Changed();
    }

    public static EnterState Door_Is_Enterable(Furniture furniture)
    {
        furniture.FurnParameters["isOpening"] = 1;
        if (furniture.FurnParameters["openness"] >= 1)
        {
            return EnterState.Yes;
        }

        return EnterState.Soon;
    }
}