"use client";

import { ProverbsCard } from "@/components/proverbs";
import { WeatherCard } from "@/components/weather";
import { MoonCard } from "@/components/moon";
import HumanApprovalCard from "@/components/humanApprovalCard";
import DeleteConfirmationCard from "@/components/deleteConfirmationCard";
import { AgentState } from "@/lib/types";
import {
  useCoAgent,
  useDefaultTool,
  useFrontendTool,
  useHumanInTheLoop,
  useRenderToolCall,
} from "@copilotkit/react-core";
import { CopilotKitCSSProperties, CopilotSidebar } from "@copilotkit/react-ui";
import { useState } from "react";
import { DynamicForm, Parameter } from "@/components/dynamicForm";

export default function CopilotKitPage() {
  const [themeColor, setThemeColor] = useState("#6366f1");

  useFrontendTool({
    name: "setThemeColor",
    parameters: [
      {
        name: "themeColor",
        description: "The theme color to set. Make sure to pick nice colors.",
        required: true,
      },
    ],
    handler({ themeColor }) {
      setThemeColor(themeColor);
    },
  });

  return (
    <main
      style={
        { "--copilot-kit-primary-color": themeColor } as CopilotKitCSSProperties
      }
    >
      <CopilotSidebar
        disableSystemMessage={true}
        clickOutsideToClose={false}
        labels={{
          title: "Popup Assistant",
          initial: "👋 Hi, there! You're chatting with an agent.",
        }}
        suggestions={[
          {
            title: "Generative UI",
            message: "Get me the weather for New York City.",
          },
          {
            title: "Frontend Tools",
            message: "Set the theme to green.",
          },
          {
            title: "Human In the Loop",
            message: "Delete the location with id 001",
          },
          {
            title: "Human In the Loop command",
            message: "Run a command to print Hello World.",
          },
          {
            title: "Write Agent State",
            message: "Add a proverb about AI.",
          },
          {
            title: "Update Agent State",
            message:
              "Please remove 1 random proverb from the list if there are any.",
          },
          {
            title: "Read Agent State",
            message: "What are the proverbs?",
          },
        ]}
      >
        <YourMainContent themeColor={themeColor} />
      </CopilotSidebar>
    </main>
  );
}

function YourMainContent({ themeColor }: { themeColor: string }) {
  // 🪁 Shared State: https://docs.copilotkit.ai/pydantic-ai/shared-state
  const { state, setState } = useCoAgent<AgentState>({
    name: "my_agent",
    initialState: {
      proverbs: [
        "CopilotKit may be new, but its the best thing since sliced bread.",
      ],
    },
  });

  // useDefaultTool({
  //   render: ({ name, args, status, result }) => {
  //     return (
  //       <div style={{ color: "black" }}>
  //         <span>
  //           {status === "complete" ? "✓" : "⏳"}
  //           {name}
  //         </span>
  //         {status === "complete" && result && (
  //           <pre>{JSON.stringify(result, null, 2)}</pre>
  //         )}
  //       </div>
  //     );
  //   },
  // });

  //🪁 Generative UI: https://docs.copilotkit.ai/pydantic-ai/generative-ui
  useRenderToolCall({
    name: "Get_Weather",
    description: "Get the weather for a given location.",
    parameters: [{ name: "location", type: "string", required: true }],
    render: ({ args, status, result }: any) => {
      if (status === "inProgress") {
        return (
          <div className="p-4 border rounded animate-pulse">
            <h3>Analyzing {args.location}...</h3>
          </div>
        );
      }
      if (status === "complete" && result) {
        return (
          <div className="p-4 border rounded bg-green-50">
            <h3>Analysis Complete: {args.location}</h3>
            {/* <pre className="mt-2 p-2 bg-gray-100 rounded">
              {JSON.stringify(result, null, 2)}
            </pre> */}
            <WeatherCard location={args.location} themeColor={themeColor} result={result} />
          </div>
        );
      }
      return <></>;
    },
  });

  // 🪁 Human In the Loop: https://docs.copilotkit.ai/pydantic-ai/human-in-the-loop
  useHumanInTheLoop(
    {
      name: "go_to_moon",
      description: "Go to the moon on request.",
      render: ({ respond, status }) => {
        return (
          <MoonCard themeColor={themeColor} status={status} respond={respond} />
        );
      },
    },
    [themeColor],
  );

  useHumanInTheLoop({
    name: "humanApprovedCommand",
    description: "Ask human for approval to run a command.",
    parameters: [
      {
        name: "command",
        type: "string",
        description: "The command to run",
        required: true,
      },
    ],
    render: ({ args, respond }) => {
      return <HumanApprovalCard args={args as any} respond={respond as any} />;
    },
  });

  useHumanInTheLoop(
    {
      name: "confirmDelete",
      description: "Ask the human to confirm deletion of an item.",
      parameters: [
        {
          name: "item",
          type: "string",
          description: "The name of the item to delete",
          required: true,
        },
        {
          name: "id",
          type: "string",
          description: "The ID of the item to delete",
          required: true,
        },
      ],
      render: ({ args, status, respond, result }) => {
        return (
          <DeleteConfirmationCard args={args as any} respond={respond as any} />
        );
      },
    },
    [themeColor],
  );

  return (
    <div
      style={{ backgroundColor: themeColor }}
      className="h-screen flex justify-center items-center flex-col transition-colors duration-300"
    >
      <ProverbsCard state={state} setState={setState} />
    </div>
  );
}
