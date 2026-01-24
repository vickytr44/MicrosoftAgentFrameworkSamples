"use client";

import React from "react";

type Props = {
  args: { item: string };
  respond?: (response: string) => void;
};

export function DeleteConfirmationCard({ args, respond }: Props) {
  if (!respond) return <></>;
  return (
    <div className="bg-white/90 dark:bg-gray-800/90 rounded-lg shadow-md p-4 max-w-md w-full">
      <div className="text-sm text-gray-700 dark:text-gray-200">
        Are you sure you want to delete
        <span className="font-semibold ml-1">{args.item}</span>?
      </div>
      <div className="mt-4 flex justify-end gap-3">
        <button
          onClick={() => respond("Delete canceled")}
          className="px-3 py-1.5 bg-gray-100 text-gray-800 rounded-md hover:bg-gray-200"
        >
          Cancel
        </button>
        <button
          onClick={() => respond("Delete confirmed")}
          className="px-3 py-1.5 bg-red-600 text-white rounded-md hover:bg-red-700"
        >
          Delete
        </button>
      </div>
    </div>
  );
}

export default DeleteConfirmationCard;
