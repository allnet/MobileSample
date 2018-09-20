using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{


    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    public float Speed = 1;
    public List<SpriteRenderer> sprites = new List<SpriteRenderer>();
    public Direction Dir = Direction.Right;


    private float heightCamera;
    private float widthCamera;

    private Vector3 PositionCam;
    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
        heightCamera = 2f * cam.orthographicSize;
        widthCamera = heightCamera * cam.aspect;
    }

    void Update()
    {
        foreach (var item in sprites)
        {
            MoveToPosition(item);

            TranslateSpritePosition(item);
        }

    }


    private void TranslateSpritePosition(SpriteRenderer target)
    {
        switch (Dir)
        {
            case Direction.Left:
                target.transform.Translate(new Vector2(Time.deltaTime * Speed * -1, 0));
                break;
            case Direction.Right:
                target.transform.Translate(new Vector2(Time.deltaTime * Speed, 0));
                break;
            case Direction.Down:
                target.transform.Translate(new Vector2(0, Time.deltaTime * Speed * -1));
                break;
            case Direction.Up:
                target.transform.Translate(new Vector2(0, Time.deltaTime * Speed));
                break;
        }
    }



    private void MoveToPosition(SpriteRenderer item)
    {
        switch (Dir)
        {

            case Direction.Left:
                if (item.transform.position.x + item.bounds.size.x / 2 < cam.transform.position.x - widthCamera / 2)
                {
                    SpriteRenderer sprite = sprites[0];
                    foreach (var i in sprites)
                    {
                        if (i.transform.position.x > sprite.transform.position.x)
                            sprite = i;
                    }

                    item.transform.position = new Vector2((sprite.transform.position.x + (sprite.bounds.size.x / 2) + (item.bounds.size.x / 2)), sprite.transform.position.y);
                }
                break;

            case Direction.Right:
                if (item.transform.position.x - item.bounds.size.x / 2 > cam.transform.position.x + widthCamera / 2)
                {
                    SpriteRenderer sprite = sprites[0];
                    foreach (var i in sprites)
                    {
                        if (i.transform.position.x < sprite.transform.position.x)
                            sprite = i;
                    }

                    item.transform.position = new Vector2((sprite.transform.position.x - (sprite.bounds.size.x / 2) - (item.bounds.size.x / 2)), sprite.transform.position.y);
                }
                break;

            case Direction.Up:
                if (item.transform.position.y - item.bounds.size.y / 2 > cam.transform.position.y + heightCamera / 2)
                {
                    SpriteRenderer sprite = sprites[0];
                    foreach (var i in sprites)
                    {
                        if (i.transform.position.y < sprite.transform.position.y)
                            sprite = i;
                    }

                    item.transform.position = new Vector2(sprite.transform.position.x, (sprite.transform.position.y - (sprite.bounds.size.y / 2) - (item.bounds.size.y / 2)));
                }

                break;

            case Direction.Down:
                if (item.transform.position.y + item.bounds.size.y / 2 < cam.transform.position.y - heightCamera / 2)
                {
                    SpriteRenderer sprite = sprites[0];
                    foreach (var i in sprites)
                    {
                        if (i.transform.position.y > sprite.transform.position.y)
                            sprite = i;
                    }

                    item.transform.position = new Vector2(sprite.transform.position.x, (sprite.transform.position.y + (sprite.bounds.size.y / 2) + (item.bounds.size.y / 2)));
                }
                break;

        }
    }

}
