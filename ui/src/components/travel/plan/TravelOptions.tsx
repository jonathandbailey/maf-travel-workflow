import { Tabs, type TabsProps } from "antd";
import { useState } from "react";
import { useArtifactHandler } from "../../../hooks/useArtifactHandler";

interface TravelOptionsProps {
    sessionId: string;
}

const TravelOptions = ({ sessionId }: TravelOptionsProps) => {
    const [tabs, setTabs] = useState<TabsProps['items']>([]);
    const [activeKey, setActiveKey] = useState<string>();

    useArtifactHandler({ sessionId, setTabs, setActiveKey });

    return (
        <>
            <div style={{ padding: "24px" }} >
                <Tabs
                    items={tabs}
                    type="card"
                    activeKey={activeKey}
                    onChange={setActiveKey}
                />
            </div>
        </>);
}

export default TravelOptions;