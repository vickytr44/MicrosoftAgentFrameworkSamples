import React from "react";

type Props = {
  args: { command: string };
  respond?: (response: string) => void;
};

export function HumanApprovalCard({ args, respond }: Props) {
  if (!respond) return <></>;
  return (
    <div className="bg-green-50/90 dark:bg-green-900/80 rounded-lg shadow-md p-4 max-w-md w-full">
      <pre className="bg-green-100 dark:bg-green-900/60 p-3 rounded-md text-sm text-green-900 overflow-x-auto">{args.command}</pre>
      <div className="mt-4 flex justify-end gap-3">
        <button
          onClick={() => respond(`Command is DENIED`)}
          className="px-3 py-1.5 bg-white border border-gray-200 text-gray-800 rounded-md hover:bg-gray-50"
        >
          Deny
        </button>
        <button
          onClick={() => respond(`Command is APPROVED`)}
          className="px-3 py-1.5 bg-green-600 text-white rounded-md hover:bg-green-700"
        >
          Approve
        </button>
      </div>
    </div>
  );
}

export default HumanApprovalCard;
