using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MrPuppet.WIP
{
    public class LocomotionController : MonoBehaviour
    {
        private CharacterController controller;

        public float speed = 5f;

        private void Start()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

            controller.Move(move * Time.deltaTime * speed);

            if (move != Vector3.zero) transform.forward = move;
        }


    }
}